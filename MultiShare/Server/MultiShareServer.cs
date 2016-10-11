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
		private const int SERVER_SEND_PORT = 49016;

		private readonly byte[] SERVER_TOKEN;

		private UdpClient _udpServer = new UdpClient(SERVER_LISTEN_PORT);

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

				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
				CancellationToken cancellationToken = cancellationTokenSource.Token;
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
						if (clientHelloData.Length != SERVER_TOKEN.Length + 6)
						{
							byte[] tokenReceived = new byte[SERVER_TOKEN.Length];
							Array.Copy(clientHelloData, tokenReceived, tokenReceived.Length);

							if (!SERVER_TOKEN.SequenceEqual(tokenReceived))
							{
								throw new Exception(@"Broadcast message does not contain ASCII ""MultiShare""");
							}
						}

						byte[] macData = new byte[6];
						PhysicalAddress mac = new PhysicalAddress(macData);

						OnNewClient(new Device(clientIP, mac));
					}
					catch (OperationCanceledException)
					{
						MessageBox.Show("Операция отменена");
					}
				}

				IsRunning = false;
			});
		}

		public Task ConnectToClient(Device device)
		{
			return Task.Run(() =>
			{
				using (TcpClient server = new TcpClient())
				{
					server.Connect(device.Address);

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
