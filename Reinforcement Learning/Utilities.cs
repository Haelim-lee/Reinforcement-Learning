using System;
using System.Collections.Generic;
using System.Linq;

namespace Reinforcement_Learning
{
	public static class Utilities
	{
		public static Random random = new Random();
		public static Dictionary<int, Dictionary<int, float>> CreateActionValueFunction()
		{
			// SARSA, Q 러닝에서 사용되는 행동 가치 함수를 초기화하는 함수

			Dictionary<int, Dictionary<int, float>> actionValueFunction = new Dictionary<int, Dictionary<int, float>>();

			for (int i = 0; i <= GameParameters.StateCount; i++)
			{
				GameState state = new GameState();
				state.PopulateBoard(i);

				if (state.IsValidSecondStage()) // 올바른 2단계 게임 보드인 경우
				{
					actionValueFunction.Add(i * 3 + 1, GetActionValueDictionary(i * 3 + 1)); // 흑돌 차례인 상태에 대한 가치 함수 엔트리 생성
					actionValueFunction.Add(i * 3 + 2, GetActionValueDictionary(i * 3 + 2)); // 백돌 차례인 상태에 대한 가치 함수 엔트리 생성

				}
				else if (state.IsValidFirstStage()) // 올바른 2단계 게임 보드인 경우
				{
					int nextTurn = state.GetFirstStageTurn();
					actionValueFunction.Add(i * 3 + nextTurn, GetActionValueDictionary(i * 3 + nextTurn)); // 현재 둘 차례인 돌을 반영한 상태에 대한 가치 함수 엔트리 생성
				}
			}

			return actionValueFunction;
		}

		public static  Dictionary<int, float> GetActionValueDictionary(int gameStateKey)
		{
			GameState gameState = new GameState(gameStateKey);
			Dictionary<int, float> actionValues = new Dictionary<int, float>();

			for (int i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++) // 1부터 9까지의 모든 행동에 대해
			{
				if (gameState.IsValidMove(i)) // 올바른 행동인 경우
				{
					actionValues.Add(i, 0.0f); // 가치 함수값을 0으로 초기화 한후 dictionary에 추가
				}
			}
			return actionValues;
		}

		public static int GetEpsilonGreedyAction(int turn, Dictionary<int, float> actionValues)
		{
			// Epsilon 탐욕 정책으로 행동을 선택하는 함수
			float greedyActionValue = 0.0f;
			float epsilon = 10;

			if (actionValues.Count == 0)
				return 0;

			if (turn == 1) // 흑돌 차례인 경우 가치 함수 최대값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Max();
			}
			else if(turn == 2) // 백돌 차례인 경우 가치 함수 최소값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Min();
			}

			int exploitRandom = random.Next(0, 100); // 랜덤값 발생
			IEnumerable<int> actionCandidates;

			if (exploitRandom < epsilon) // 탐험을 하는 경우
			{
				// 선택되지 않은 가치 함수값을 가지는 행동들을 선택
				actionCandidates = actionValues.Where(e => e.Value != greedyActionValue).Select(e => e.Key);
				if(actionCandidates.Count() == 0) // 만일 선택된 행동이 없으면 (가치함수값이 모두 똑같은 경우), 전체 행동 고려
					actionCandidates = actionValues.Where(e => e.Value == greedyActionValue).Select(e => e.Key);
			}
			else // 탐험하지 않는 경우
			{
				// 선택된 가치 함수값을 가지는 행동들을 선택
				actionCandidates = actionValues.Where(e => e.Value == greedyActionValue).Select(e => e.Key);
			}

			// 선택된 행동들 중 하나를 랜덤하게 선택해서 반환
			return actionCandidates.ElementAt(random.Next(0, actionCandidates.Count()));
		}

		public static int GetGreedyAction(int turn, Dictionary<int, float> actionValues)
		{
			// 주어진 가치함수 dictionary로부터 turn을 고려하여 행동을 선택. 흑돌 차례이면 가치함수값이 최대값인 행동들을, 백돌 차례이면 최소값인 행동들을 선택
			IEnumerable<int> actionCandidates = GetGreedyActionCandidate(turn, actionValues);

			if (actionCandidates.Count() == 0)
				return 0;

			// 선택된 행동 중 랜덤하게 하나를 선택하여 반환
			return actionCandidates.ElementAt(random.Next(0, actionCandidates.Count()));
		}

		public static IEnumerable<int> GetGreedyActionCandidate(int turn, Dictionary<int, float> actionValues)
		{
			float greedyActionValue = 0.0f;

			if (actionValues.Count == 0)
				return new List<int>();

			if (turn == 1) // 흑돌 차례이면 주어진 가치 함수값 중 최대값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Max();
			}
			else if (turn == 2) // 백돌 차례이면 주어진 가치 함수값 중 최소값 선택
			{
				greedyActionValue = actionValues.Select(e => e.Value).Min();
			}

			// 선택된 가치 함수값을 가지는 행동들을 선택해서 반환
			return actionValues.Where(e => e.Value == greedyActionValue).Select(e => e.Key);
		}

		public static float GetGreedyActionValue(int turn, Dictionary<int, float> actionValues)
		{
			// 주어진 dictionary에 element가 없으면 0 반환
			if (actionValues.Count == 0)
				return 0.0f;

			if (turn == 1) // 흑돌의 차례이면 주어진 dictionary의 가치 함수값 중 최대값을 반환
			{
				return actionValues.Select(e => e.Value).Max();
			}
			else if (turn == 2) // 백돌의 차례이면 주어진 dictionary의 가치 함수값 중 최소값을 반환
			{
				return actionValues.Select(e => e.Value).Min();
			}

			return 0.0f;
		}

		public static float EvaluateValueFunction(QFunctionType functionType)
		{
			// 모든 게임 상태에 대해 SARSA나 Q 러닝 에이전트가 동적 프로그래밍 에이전트와 같은 행동을 선택하는 비율을 계산하여 반환하는 함수

			if (MainProgram.ValueFunctionManager.StateValueFunction.Count == 0)
				return 0.0f;

			int totalStateCount = 0;
			int matchingStateCount = 0;

			for (int i = 0; i <= GameParameters.StateCount; i++)
			{
				GameState state = new GameState();
				state.PopulateBoard(i);

				if (state.IsValidSecondStage())
				{
					GameState gameState = new GameState(i * 3 + 1);
					if (!gameState.isFinalState() && gameState.CountValidMoves() > 0)
					{
						if (CompareActionCandidate(i * 3 + 1, functionType))
						{
							matchingStateCount++;
						}
						totalStateCount++;
					}

					gameState = new GameState(i * 3 + 2);
					if (!gameState.isFinalState() && gameState.CountValidMoves() > 0)
					{
						if (CompareActionCandidate(i * 3 + 2, functionType))
						{
							matchingStateCount++;
						}
						totalStateCount++;
					}
				}
				else if (state.IsValidFirstStage())
				{
					int nextTurn = state.GetFirstStageTurn();
					GameState gameState = new GameState(i * 3 + nextTurn);
					if (!gameState.isFinalState() && gameState.CountValidMoves() > 0)
					{
						if (CompareActionCandidate(i * 3 + nextTurn, functionType))
						{
							matchingStateCount++;
						}
						totalStateCount++;
					}
				}
			}
			return ((float)matchingStateCount) / ((float)totalStateCount) * 100.0f;
		}

		public static bool CompareActionCandidate(int boardStateKey, QFunctionType functionType)
		{
			// 주어진 상태에 대해서 SARSA나 Q 러닝 에이전트가 선택하게 되는 행동을 동적 프로그래밍 에이전트도 마찬가지로 선택하는지를 판단하는 함수

			IEnumerable<int> DPActionCandidate = MainProgram.ValueFunctionManager.GetNextMoveCandidate(boardStateKey);
			IEnumerable<int> QActionCandidate;

			if (functionType == QFunctionType.SARSA)
				QActionCandidate = MainProgram.SarsaValueFunctionManager.GetNextMoveCandidate(boardStateKey);
			else if (functionType == QFunctionType.QLEARNING)
				QActionCandidate = MainProgram.QLearningValueFunctionManager.GetNextMoveCandidate(boardStateKey);
			else
				QActionCandidate = new List<int>();

			if (QActionCandidate.Count() == 0 && DPActionCandidate.Count() > 0)
				return false;

			IEnumerable<int> UnmatchedActionList = QActionCandidate.Where(e => !DPActionCandidate.Contains(e));

			if (UnmatchedActionList.Count() == 0)
				return true;

			return false;
		}
	}
}
