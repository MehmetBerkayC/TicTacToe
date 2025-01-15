using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;

    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winnerPlayerType;
    }

    public event EventHandler OnRematch;
    public event EventHandler OnGameTie;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnObjectPlaced;

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }
    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalLeft,
        DiagonalRight,
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private PlayerType _localPlayerType;
    private NetworkVariable<PlayerType> _currentPlayablePlayerType = new NetworkVariable<PlayerType>();

    private PlayerType[,] _playerTypeArray;

    private List<Line> _lineList;

    private NetworkVariable<int> _playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> _playerCircleScore = new NetworkVariable<int>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        _playerTypeArray = new PlayerType[3, 3];

        _lineList = new List<Line>
        {
            /// Horizontal
            // Bottom Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)},
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal,
            },
            
            // Middle Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal,
            },
            
            // Top Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)},
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal,
            },

            /// Vertical
            // Left Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)},
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical,
            },
            
            // Middle Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical,
            },
            
            // Right Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2)},
                centerGridPosition = new Vector2Int(2, 1),
                orientation= Orientation.Vertical,
            },

            /// Diagonals
            // Left Diag Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation= Orientation.DiagonalRight,
            },
            // Right Diag Line
            new Line {
                gridVector2IntList = new List<Vector2Int>{ new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation= Orientation.DiagonalLeft,
            },
        };
    }

    // Only runs on a connection start
    public override void OnNetworkSpawn()
    {
        //Debug.Log(NetworkManager.Singleton.LocalClientId);
        Debug.Log(NetworkManager.Singleton.LocalClientId + " " + IsServer);
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            _localPlayerType = PlayerType.Cross;
        }
        else
        {
            _localPlayerType = PlayerType.Circle;
        }
        Debug.Log(_localPlayerType);

        if (IsServer) // Connection event sub
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        // Network Variable Change events -> there is always a delay on network related changes
        // Both Server and Client will have the results
        _currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        _playerCrossScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };

        _playerCircleScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            //Start Game
            _currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        /// ALL OF THIS GAME IS SERVER AUTHORITATIVE
        //Debug.Log("Clicked on grid position: " + x + "," + y);
        // Check player turn
        if (playerType != _currentPlayablePlayerType.Value) { return; }

        // Check valid positon
        if (_playerTypeArray[x, y] != PlayerType.None) { return; }

        // Play
        TriggerOnObjectPlacedRpc();
        _playerTypeArray[x, y] = playerType;

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType,
        });

        // End player turn
        switch (_currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                _currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                _currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        // Only the server tests
        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnObjectPlacedRpc()
    {
        OnObjectPlaced?.Invoke(this, EventArgs.Empty);
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            _playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            _playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            _playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
            );
    }


    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        //Debug.Log(aPlayerType != PlayerType.None && aPlayerType == bPlayerType && bPlayerType == cPlayerType);
        return aPlayerType != PlayerType.None && aPlayerType == bPlayerType && bPlayerType == cPlayerType;
    }

    private void TestWinner()
    {
        for (int i = 0; i < _lineList.Count; i++)
        {
            Line line = _lineList[i];
            if (TestWinnerLine(line))
            {
                // Win
                //Debug.Log("Winner!");
                _currentPlayablePlayerType.Value = PlayerType.None;

                // Score
                PlayerType winnerPlayerType = _playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];
                switch (winnerPlayerType)
                {
                    case PlayerType.Cross:
                        _playerCrossScore.Value++;
                        break;
                    case PlayerType.Circle:
                        _playerCircleScore.Value++;
                        break;
                }

                // Trigger the event on all clients
                TriggerOnGameWinRpc(i, winnerPlayerType);
                return;
            }
        }

        bool hasTie = true;

        for (int x = 0; x < _playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < _playerTypeArray.GetLength(1); y++)
            {
                if (_playerTypeArray[x, y] == PlayerType.None)
                {
                    hasTie = false;
                }
            }
        }

        if (hasTie)
        {
            TriggerOnGameTieRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTieRpc()
    {
        OnGameTie?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winnerPlayerType)
    {
        Line line = _lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = line,
            // _playerTypeArray data is only on the server
            winnerPlayerType = winnerPlayerType
        });
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int x = 0; x < _playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < _playerTypeArray.GetLength(1); y++)
            {
                _playerTypeArray[x, y] = PlayerType.None;
            }
        }

        _currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }

    public PlayerType GetLocalPlayerType()
    {
        return _localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return _currentPlayablePlayerType.Value;
    }

    public void GetScores(out int playerCrossScore, out int playerCircleScore)
    {
        playerCrossScore = _playerCrossScore.Value;
        playerCircleScore = _playerCircleScore.Value;
    }
}
