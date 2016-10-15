using MultiShare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
		//private const int SERVER_LISTEN_PORT = 41033;
		private const int SERVER_SEND_PORT = 49016;

		private const int MAC_LENGTH = 6;

		private readonly byte[] SERVER_TOKEN;

		private UdpClient _udpServer = new UdpClient(SERVER_LISTEN_PORT);

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
						//_udpServer.Client.Bind(new IPEndPoint(IPAddress.Any, SERVER_))
						//_udpServer.EnableBroadcast = true;
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
						Buffer.BlockCopy(clientHelloData, 0, macData, 0, MAC_LENGTH);
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

		public Task ConnectToClient(Device device)
		{
			return Task.Run(() =>
			{
				using (TcpClient server = new TcpClient())
				{
					server.Connect(device.Address, SERVER_SEND_PORT);

					using (NetworkStream ns = server.GetStream())
					{
						BinaryWriter bw = new BinaryWriter(ns, Encoding.ASCII);
						bw.Write(SERVER_TOKEN);
					}
				}
			});
		}
	}
}
