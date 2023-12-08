using System;
using System.Collections.Generic;
using System.Linq;

namespace Reinforcement_Learning
{
	public class GameParameters
	{
		public static int StateCount = 19560; // 3진수 222211110의 10진수 표현이 19560
		public static int ActionMinIndex = 1;
		public static int ActionMaxIndex = 9;
	}

	public class GameState
	{
		public int[,] BoardState;
		public int NextTurn;
		public int BoardStateKey;
		public int NumberOfBlacks;
		public int NumberOfWhites;
		public int GameWinner;

		public class ConnectedPosition
		{
			public int row;
			public int col;
			public ConnectedPosition(int r, int c)
			{
				row = r;
				col = c;
			}
		}

		public GameState()
		{
			// 초기 게임 상태로 클래스를 초기화
			// 흑돌의 차례에서 아무것도 보드에 놓여져 있지 않은 상태

			BoardState = new int[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
			NextTurn = 1;
			BoardStateKey = 1;
			NumberOfBlacks = 0;
			NumberOfWhites = 0;
			GameWinner = 0;
		}

		public GameState(int boardStateKey)
		{
			// 주어진 게임 상태 키로부터 게임 상태를 생성하면서 클래스를 초기화

			BoardState = new int[3, 3];
			BoardStateKey = boardStateKey;
			NextTurn = boardStateKey % 3;
			GameWinner = 0;
			PopulateBoard(boardStateKey / 3);
		}

		public void PopulateBoard(int boardState)
		{
			// 주어진 보드 상태 값을 3진수로 변환시키면서 보드 상태 생성

			int boardValueProcessing = boardState;
			NumberOfBlacks = 0;
			NumberOfWhites = 0;

			for (int i = 8; i >= 0; i--)
			{
				int boardValue = boardValueProcessing % 3;
				boardValueProcessing = boardValueProcessing / 3;

				BoardState[i / 3, i % 3] = boardValue;

				if (boardValue == 1)
					NumberOfBlacks++;

				if (boardValue == 2)
					NumberOfWhites++;
			}
		}

		public bool IsValidSecondStage()
		{
			// 올바른 2단계 게임 보드인지 판단. 1과 2의 개수가 4이면 올바른 2단계 게임 보드

			if (NumberOfBlacks == 4 && NumberOfWhites == 4)
				return true;

			return false;
		}

		public bool IsValidFirstStage()
		{
			// 올바른 1단계 게임 보드인지 판단. 1의 개수는 4 이하, 2의 개수는 3 이하여야 하며, 1과 2의 개수가 같거나 1이 하나 많아야 한다.

			if (NumberOfBlacks > 4)
				return false;
			if (NumberOfWhites > 3)
				return false;

			if (NumberOfBlacks == NumberOfWhites || NumberOfBlacks == NumberOfWhites + 1)
				return true;

			return false;
		}

		public int GetFirstStageTurn()
		{
			// 1단계 게임 보드인 경우 어느 돌이 둘 차례인지를 알아보는 함수
			// 1과 2가 개수가 같으면 1이 둘 차례, 1이 하나 많으면 2가 둘 차례

			if (NumberOfBlacks == NumberOfWhites)
				return 1;
			if (NumberOfBlacks == NumberOfWhites + 1)
				return 2;

			return 0;
		}

		public bool isFinalState()
		{
			// 게임이 끝난 상태인지 확인

			GameWinner = 0;

			if (BoardState[0, 0] == BoardState[0, 1] && BoardState[0, 1] == BoardState[0, 2])
			{
				if (BoardState[0, 0] != 0)
				{
					GameWinner = BoardState[0, 0];
					return true;
				}
			}
			if (BoardState[1, 0] == BoardState[1, 1] && BoardState[1, 1] == BoardState[1, 2])
			{
				if (BoardState[1, 0] != 0)
				{
					GameWinner = BoardState[1, 0];
					return true;
				}
			}
			if (BoardState[2, 0] == BoardState[2, 1] && BoardState[2, 1] == BoardState[2, 2])
			{
				if (BoardState[2, 0] != 0)
				{
					GameWinner = BoardState[2, 0];
					return true;
				}
			}
			if (BoardState[0, 0] == BoardState[1, 0] && BoardState[1, 0] == BoardState[2, 0])
			{
				if (BoardState[0, 0] != 0)
				{
					GameWinner = BoardState[0, 0];
					return true;
				}
			}
			if (BoardState[0, 1] == BoardState[1, 1] && BoardState[1, 1] == BoardState[2, 1])
			{
				if (BoardState[0, 1] != 0)
				{
					GameWinner = BoardState[0, 1];
					return true;
				}
			}
			if (BoardState[0, 2] == BoardState[1, 2] && BoardState[1, 2] == BoardState[2, 2])
			{
				if (BoardState[0, 2] != 0)
				{
					GameWinner = BoardState[0, 2];
					return true;
				}
			}
			if (BoardState[0, 0] == BoardState[1, 1] && BoardState[1, 1] == BoardState[2, 2])
			{
				if (BoardState[0, 0] != 0)
				{
					GameWinner = BoardState[0, 0];
					return true;
				}
			}
			if (BoardState[0, 2] == BoardState[1, 1] && BoardState[1, 1] == BoardState[2, 0])
			{
				if (BoardState[0, 2] != 0)
				{
					GameWinner = BoardState[0, 2];
					return true;
				}
			}

			return false;
		}

		public float GetReward()
		{
			// 보상값 함수
			// 게임을 1이 이긴 상태이면 100, 2가 이긴 상태이면 -100, 그렇지 않으면 0 반환

			if (isFinalState())
			{
				if (GameWinner == 1)
					return 100.0f;
				else if (GameWinner == 2)
					return -100.0f;
			}

			return 0.0f;
		}

		public int CountValidMoves()
		{
			// 현재 게임 상태에 대해 올바른 행동의 개수를 찾아보는 함수

			int count = 0;
			for (int i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++) // 1부터 9까지의 모든 행동에 대해
			{
				if (IsValidMove(i))
					count++;
			}
			return count;
		}

		public bool IsValidMove(int move)
		{
			// 현재 주어진 게임 상태에 대해 주어진 행동을 적용할 수 있는지 판단하는 함수

			int row = (move - 1) / 3;
			int col = (move - 1) % 3;

			if (IsValidFirstStage()) // 1단계 게임인 경우 주어진 위치가 빈칸인지 확인
			{
				if (BoardState[row, col] == 0)
					return true;
			}
			else if (IsValidSecondStage()) // 2단계 게임인 경우 주어진 위치에 현재 차례인 돌이 놓여 있고,
			{
				if (BoardState[row, col] != NextTurn)
					return false;

				IEnumerable<ConnectedPosition> ConnectedPositions = GetConnectedPosition(row, col);
				IEnumerable<ConnectedPosition> ConnectedEmptySpots = ConnectedPositions.Where(e => BoardState[e.row, e.col] == 0);

				if (ConnectedEmptySpots.Count() > 0)  // 그 돌이 빈칸에 연결되어 있으며, 
				{
					IEnumerable<ConnectedPosition> ConnectedOpponents = ConnectedPositions.Where(e => BoardState[e.row, e.col] != 0 && BoardState[e.row, e.col] != NextTurn);

					if(ConnectedOpponents.Count() > 0) // 돌이 빈칸으로 이동해 간 후 비게 된 원래 자리에 상대방 돌이 움직여 올 수 있는지 판단
					{
						return true;
					}
				}
			}
			return false;
		}

		public GameState GetNextState(int move)
		{
			// 현재 게임 상태에 대해 주어진 행동을 취하여 전이되어 가는 게임 상태를 생성해서 반환
			GameState nextState = new GameState(BoardStateKey);
			nextState.MakeMove(move);
			return nextState;
		}

		public void MakeMove(int move)
		{
			// 현재 게임 상태에 행동을 적용하는 함수

			int row = (move - 1) / 3;
			int col = (move - 1) % 3;

			if (IsValidFirstStage())
			{
				// 첫번째 단계인 경우 주어진 위치에 다음 차례에 둘 돌을 놓는 방식으로 상태를 업데이트

				BoardState[row, col] = NextTurn;

				if (NextTurn == 1)
					NumberOfBlacks++;
				else if (NextTurn == 2)
					NumberOfWhites++;
			}
			else if (IsValidSecondStage())
			{
				// 두번째 단계인 경우 주어진 위치의 돌을 빈칸으로 옮겨가는 방식으로 상태를 업데이트
				int emptyRow = 0;
				int emptyCol = 0;

				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						if (BoardState[i, j] == 0)
						{
							emptyRow = i;
							emptyCol = j;
							break;
						}
					}
				}
				BoardState[row, col] = 0;
				BoardState[emptyRow, emptyCol] = NextTurn;
			}

			if (NextTurn == 1)
				NextTurn = 2;
			else if (NextTurn == 2)
				NextTurn = 1;

			int boardStateKey = 0;

			for (int i = 0; i < 9; i++)
			{
				boardStateKey = boardStateKey * 3;
				boardStateKey = boardStateKey + BoardState[i / 3, i % 3];
			}

			BoardStateKey = boardStateKey * 3 + NextTurn;
		}

		private IEnumerable<ConnectedPosition> GetConnectedPosition(int row, int col)
		{
			// 주어진 위치에서 게임 보드 상에서 연결된 격자점들의 좌표들을 반환해주는 함수
			// 2단계 게임 보드에서 주어진 행동이 올바른지 판단하는 과정에서 사용되는 함수

			List<ConnectedPosition> connectedPositionList = new List<ConnectedPosition>();

			if (row == 0 && col == 0)
			{
				connectedPositionList.Add(new ConnectedPosition(0, 1));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(1, 0));
			}
			if (row == 0 && col == 1)
			{
				connectedPositionList.Add(new ConnectedPosition(0, 0));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(0, 2));
			}
			if (row == 0 && col == 2)
			{
				connectedPositionList.Add(new ConnectedPosition(0, 1));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(1, 2));
			}
			if (row == 1 && col == 0)
			{
				connectedPositionList.Add(new ConnectedPosition(0, 0));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(2, 0));
			}
			if (row == 1 && col == 1)
			{
				connectedPositionList.Add(new ConnectedPosition(0, 0));
				connectedPositionList.Add(new ConnectedPosition(0, 1));
				connectedPositionList.Add(new ConnectedPosition(0, 2));
				connectedPositionList.Add(new ConnectedPosition(1, 0));
				connectedPositionList.Add(new ConnectedPosition(1, 2));
				connectedPositionList.Add(new ConnectedPosition(2, 0));
				connectedPositionList.Add(new ConnectedPosition(2, 1));
				connectedPositionList.Add(new ConnectedPosition(2, 2));
			}
			if (row == 1 && col == 2)
			{
				connectedPositionList.Add(new ConnectedPosition(0, 2));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(2, 2));
			}
			if (row == 2 && col == 0)
			{
				connectedPositionList.Add(new ConnectedPosition(1, 0));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(2, 1));
			}
			if (row == 2 && col == 1)
			{
				connectedPositionList.Add(new ConnectedPosition(2, 0));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(2, 2));
			}
			if (row == 2 && col == 2)
			{
				connectedPositionList.Add(new ConnectedPosition(1, 2));
				connectedPositionList.Add(new ConnectedPosition(1, 1));
				connectedPositionList.Add(new ConnectedPosition(2, 1));
			}
			return connectedPositionList;
		}

		public void DisplayBoard(int turnCount, int lastMove, GamePlayer blackPlayer, GamePlayer whitePlayer)
		{
			// 화면에 현재 게임 상태를 출력하는 함수. 게임 진행 과정에서 사용됨
			Console.Clear();
			Console.WriteLine(Environment.NewLine);

			Console.WriteLine($"X: {blackPlayer}, O: {whitePlayer}");
			Console.Write($"게임 턴: {turnCount}, ");			
			Console.WriteLine($" {GetTurnMark()} 차례입니다.");
			Console.WriteLine(Environment.NewLine);

			if (IsValidFirstStage())
			{
				Console.Write("1단계 진행중입니다.");
			}
			else
			{
				Console.Write("2단계 진행중입니다.");
			}

			if(lastMove != 0)
			{
				Console.WriteLine($" 지난 행동, Row: {(lastMove - 1) / 3}, Column: {(lastMove - 1) % 3}");
			}
			Console.WriteLine(Environment.NewLine);

			Console.WriteLine($"    {GetGameBoardValue(0, 0)}          {GetGameBoardValue(0, 1)}          {GetGameBoardValue(0, 2)}");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine($"    {GetGameBoardValue(1, 0)}          {GetGameBoardValue(1, 1)}          {GetGameBoardValue(1, 2)}");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine("");
			Console.WriteLine($"    {GetGameBoardValue(2, 0)}          {GetGameBoardValue(2, 1)}          {GetGameBoardValue(2, 2)}");

			isFinalState();
			Console.WriteLine(Environment.NewLine);
			switch (GameWinner)
			{
				case 1:
					Console.WriteLine("X 가 이겼습니다!!");
					break;
				case 2:
					Console.WriteLine("O 가 이겼습니다!!");
					break;
				default:
					Console.WriteLine("게임 진행중입니다!!");
					break;
			}
		}

		private string GetTurnMark()
		{
			return NextTurn == 1 ? "X" : "O";
		}

		private string GetGameBoardValue(int row, int col)
		{
			switch(BoardState[row,col])
			{
				case 1:
					return "X";
				case 2:
					return "O";
				default:
					return "+";
			}
		}
	}

}
