using MultiShare.Model;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiShare.Server
{
	public class NewClientEventArgs : EventArgs
	{
		private Device _client;
		public Device Device
		{
			get { return _client; }
		}

		public NewClientEventArgs(Device device)
		{
			_client = device;
		}
	}

	public class MultiShareServer
	{
		private const int SERVER_LISTEN_PORT = 49015;
		private const int SERVER_SEND_PORT = 49016;

		private const int MAC_LENGTH = 6;

		private readonly byte[] SERVER_TOKEN;

		private UdpClient _udpServer = new UdpClient(SERVER_LISTEN_PORT);

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private BinaryWriter _currentWriter = null;
		private TcpClient _server = null;

		public bool IsConnected
		{
			get
			{
				return _server != null && _currentWriter != null;
			}
		}

		private bool _isRunning = false;
		public bool IsRunning
		{
			get { return _isRunning; }
			private set
			{
				if (_isRunning != value)
				{
					_isRunning = value;
				}
			}
		}

		public event EventHandler<NewClientEventArgs> NewClient;

		protected virtual void OnNewClient(Device device)
		{
			NewClient?.Invoke(this, new NewClientEventArgs(device));
		}

		public MultiShareServer()
		{
			SERVER_TOKEN = Encoding.ASCII.GetBytes(nameof(MultiShare));
		}

		public Task StartAsync()
		{
			return Task.Run(() =>
			{
				IsRunning = true;

				CancellationToken cancellationToken = _cancellationTokenSource.Token;
				IPEndPoint clientIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] clientHelloData;
				Task<UdpReceiveResult> udpReceiveTask;

				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						udpReceiveTask = _udpServer.ReceiveAsync();
						udpReceiveTask.Wait(cancellationToken);

						UdpReceiveResult udpReceiveResult = udpReceiveTask.Result;
						clientIP = udpReceiveResult.RemoteEndPoint;
						clientHelloData = udpReceiveResult.Buffer;

						// Проверить длину пакета (10 символов маркера + 6 байтов MAC)
						if (clientHelloData.Length != SERVER_TOKEN.Length + MAC_LENGTH)
						{
							byte[] tokenReceived = new byte[SERVER_TOKEN.Length];
							Array.Copy(clientHelloData, tokenReceived, tokenReceived.Length);

							if (!SERVER_TOKEN.SequenceEqual(tokenReceived))
							{
								throw new Exception(@"Широковещательное сообщение не содержит ASCII ""MultiShare""");
							}
						}

						byte[] macData = new byte[MAC_LENGTH];
						Buffer.BlockCopy(clientHelloData, 10, macData, 0, MAC_LENGTH);
						PhysicalAddress mac = new PhysicalAddress(macData);

						OnNewClient(new Device(clientIP.Address, mac));
					}
					catch (OperationCanceledException)
					{
						throw new Exception("Операция отменена");
					}
				}

				IsRunning = false;
			});
		}

		public void StopServer()
		{
			_cancellationTokenSource.Cancel();
		}

		public Task SendMessage(string message)
		{
			return Task.Run(() =>
			{
				if (_currentWriter != null)
				{
					_currentWriter.Write(message);
				}
			});
		}

		public Task ConnectToClient(Device device)
		{
			return Task.Run(() =>
			{
				TcpClient server = new TcpClient();
				server.Connect(device.Address, SERVER_SEND_PORT);
				NetworkStream ns = server.GetStream();
				BinaryWriter bw = new BinaryWriter(ns, Encoding.UTF8);
				bw.Write(SERVER_TOKEN);
				_currentWriter = bw;
				_server = server;
			});
		}

		public void DisconnectFromClient()
		{
			if (_server == null || _currentWriter == null)
			{
				throw new NotSupportedException("Server did not connect to any client!");
			}

			_currentWriter.Close();
			_server.Close();
			_currentWriter = null;
			_server = null;
		}
	}
}
