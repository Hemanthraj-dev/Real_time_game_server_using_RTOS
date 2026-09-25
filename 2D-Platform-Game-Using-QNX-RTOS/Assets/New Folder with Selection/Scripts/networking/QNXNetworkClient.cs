using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace QNXNetworking
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerInputPacket
    {
        public uint playerId;
        public uint sequence;
        public float moveX;
        public float moveY;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerStateInfo
    {
        public uint playerId;
        public float posX;
        public float posY;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldStateSnapshot
    {
        public uint tickSequence;
        public uint playerCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public PlayerStateInfo[] players;
    }

    public class QNXMultiplayerClient : MonoBehaviour
    {
        [Header("Network Settings")]
        [SerializeField] private string serverIp = "192.168.1.28";
        [SerializeField] private int serverPort = 7777;
        [SerializeField] private uint myPlayerId = 1; // Set to 2 for the second client instance

        [Header("Player References")]
        [SerializeField] private Transform player1Transform;
        [SerializeField] private Transform player2Transform;
        [SerializeField] private float lerpSpeed = 15.0f;

        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private Thread receiveThread;
        private bool isRunning = false;

        private uint inputSequence = 0;
        private float targetP1X, targetP2X;
        private readonly object stateLock = new object();

        private void Start()
        {
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            try
            {
                serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
                udpClient = new UdpClient();
                udpClient.Connect(serverEndPoint);

                isRunning = true;
                receiveThread = new Thread(ReceiveLoop) { IsBackground = true, Name = "QNX_Multiplayer_Receive" };
                receiveThread.Start();

                Debug.Log($"[QNX Client] Connected as Player {myPlayerId} to {serverIp}:{serverPort}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QNX Client] Init Failed: {ex.Message}");
            }
        }

        private void Update()
        {
            // 1. Send local player input to QNX Master Server
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            SendInputPacket(moveX, moveY);

            // 2. Read latest synchronized X positions from server snapshot
            float p1X, p2X;
            lock (stateLock)
            {
                p1X = targetP1X;
                p2X = targetP2X;
            }

            // 3. Smoothly interpolate both players (Unity handles local Y gravity/jumping)
            if (player1Transform != null)
            {
                Vector3 pos1 = player1Transform.position;
                pos1.x = Mathf.Lerp(pos1.x, p1X, Time.deltaTime * lerpSpeed);
                player1Transform.position = pos1;
            }

            if (player2Transform != null)
            {
                Vector3 pos2 = player2Transform.position;
                pos2.x = Mathf.Lerp(pos2.x, p2X, Time.deltaTime * lerpSpeed);
                player2Transform.position = pos2;
            }
        }

        private void SendInputPacket(float moveX, float moveY)
        {
            if (udpClient == null || !isRunning) return;

            inputSequence++;
            PlayerInputPacket inputPacket = new PlayerInputPacket
            {
                playerId = myPlayerId,
                sequence = inputSequence,
                moveX = moveX,
                moveY = moveY
            };

            byte[] bytes = StructureToBytes(inputPacket);
            udpClient.Send(bytes, bytes.Length);
        }

        private void ReceiveLoop()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                try
                {
                    byte[] receivedBytes = udpClient.Receive(ref remoteEP);
                    if (receivedBytes.Length >= 8) // At least header
                    {
                        WorldStateSnapshot snapshot = BytesToStructure<WorldStateSnapshot>(receivedBytes);
                        
                        lock (stateLock)
                        {
                            for (int i = 0; i < snapshot.playerCount && i < 2; i++)
                            {
                                if (snapshot.players[i].playerId == 1)
                                    targetP1X = snapshot.players[i].posX;
                                else if (snapshot.players[i].playerId == 2)
                                    targetP2X = snapshot.players[i].posX;
                            }
                        }
                    }
                }
                catch (SocketException) { }
                catch (Exception ex) { Debug.LogWarning($"[QNX Receive Error]: {ex.Message}"); }
            }
        }

        private static byte[] StructureToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] byteArray = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, true);
                Marshal.Copy(ptr, byteArray, 0, size);
            }
            finally { Marshal.FreeHGlobal(ptr); }
            return byteArray;
        }

        private static T BytesToStructure<T>(byte[] byteArray) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            T structure;
            try
            {
                Marshal.Copy(byteArray, 0, ptr, size);
                structure = Marshal.PtrToStructure<T>(ptr);
            }
            finally { Marshal.FreeHGlobal(ptr); }
            return structure;
        }

        private void OnDestroy()
        {
            isRunning = false;
            udpClient?.Close();
            if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();
        }
    }
}