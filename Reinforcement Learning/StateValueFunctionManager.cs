using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;


namespace Reinforcement_Learning
{
	public class StateValueFunctionManager
	{
		public Dictionary<int, float> StateValueFunction { get; set; } // 상태 가치 함수값들을 저장하기 위한 Dictionary
		public float DiscountFactor = 0.9f;
		public string stateValueFunctionFilePath = "StateValueFunction.json";

		int num00 = 0;
		int num10 = 0;
		int num11 = 0;
		int num21 = 0;
		int num22 = 0;
		int num32 = 0;
		int num33 = 0;
		int num43 = 0;
		int num44 = 0;

		public StateValueFunctionManager()
		{
			StateValueFunction = new Dictionary<int, float>();
		}

		public void UpdateByDynamicProgramming()
		{            
			InitializeValueFunction();

			ApplyDynamicProgramming();
		}

		public void InitializeValueFunction()
		{
			Console.Clear();
			Console.WriteLine("동적 프로그래밍 시작");
			Console.WriteLine("가치 함수 초기화");

			StateCountReset();
			StateValueFunction.Clear();

			for (int i = 0; i <= GameParameters.StateCount; i++)
			{
				GameState state = new GameState();
				state.PopulateBoard(i);

				if(state.IsValidSecondStage()) // 2단계 게임 보드일 경우
				{
					StateValueFunction.Add(i * 3 + 1, 0.0f); // 흑돌이 둘 차례인 상태를 가치 함수 엔트리로 추가
					StateValueFunction.Add(i * 3 + 2, 0.0f); // 백돌이 둘 차례인 상태를 가치 함수 엔트리로 추가

					if (state.NumberOfBlacks == 4 && state.NumberOfWhites == 4)
						num44++;
				}
				else if(state.IsValidFirstStage()) // 1단계 게임 보드일 경우
				{
					StateValueFunction.Add(i * 3 + state.GetFirstStageTurn(), 0.0f); // 어느 둘이 둘 차례인지를 파악하여 해당 상태를 가치 함수 엔트리로 추가

					if (state.NumberOfBlacks == 0 && state.NumberOfWhites == 0)
						num00++;
					if (state.NumberOfBlacks == 1 && state.NumberOfWhites == 0)
						num10++;
					if (state.NumberOfBlacks == 1 && state.NumberOfWhites == 1)
						num11++;
					if (state.NumberOfBlacks == 2 && state.NumberOfWhites == 1)
						num21++;
					if (state.NumberOfBlacks == 2 && state.NumberOfWhites == 2)
						num22++;
					if (state.NumberOfBlacks == 3 && state.NumberOfWhites == 2)
						num32++;
					if (state.NumberOfBlacks == 3 && state.NumberOfWhites == 3)
						num33++;
					if (state.NumberOfBlacks == 4 && state.NumberOfWhites == 3)
						num43++;

				}
			}

			Console.WriteLine(Environment.NewLine);
			Console.WriteLine("가치 함수 초기화 완료");
			Console.WriteLine(Environment.NewLine);
			Console.WriteLine($"Black 0, White 0: {num00}");
			Console.WriteLine($"Black 1, White 0: {num10}");
			Console.WriteLine($"Black 1, White 1: {num11}");
			Console.WriteLine($"Black 2, White 1: {num21}");
			Console.WriteLine($"Black 2, White 2: {num22}");
			Console.WriteLine($"Black 3, White 2: {num32}");
			Console.WriteLine($"Black 3, White 3: {num33}");
			Console.WriteLine($"Black 4, White 3: {num43}");
			Console.WriteLine($"Black 4, White 4: {num44}");
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public void ApplyDynamicProgramming()
		{
			Console.Clear();
			Console.WriteLine("동적 프로그래밍 적용");
			Console.WriteLine(Environment.NewLine);

			int loopCount = 0;
			bool terminateLoop = false;

			while(!terminateLoop)
			{
				Dictionary<int, float> nextStateValueFunction = new Dictionary<int, float>(); // 업데이트되는 가치 함수값을 임시로 저장하기 위한 dictionary
				float valueFunctionUpdateAmount = 0.0f; // 동적 프로그래밍 각 단계에서 함수값이 업데이트된 크기

				foreach(KeyValuePair<int, float> valueFunctionEntry in StateValueFunction) // 가치 함수 업데이트 루프
				{
					float updatedValue = UpdateValueFunction(valueFunctionEntry.Key); // 가치 함수 업데이트 계산
					float updatedAmount = Math.Abs(valueFunctionEntry.Value - updatedValue); // 업데이트 크기
					nextStateValueFunction[valueFunctionEntry.Key] = updatedValue; // 가치 함수 업데이트

					if (updatedAmount > valueFunctionUpdateAmount) // 루프를 돌면서 함수가 업데이트된 크기를 기록
						valueFunctionUpdateAmount = updatedAmount;
				}

				StateValueFunction = new Dictionary<int, float>(nextStateValueFunction); // 가치 함수값을 임시 저장 가치 함수로 변경

				loopCount++;
				Console.WriteLine($"동적 프로그래밍 {loopCount}회 수행, 업데이트 오차 {valueFunctionUpdateAmount}");

				if (valueFunctionUpdateAmount < 0.01f) // 업데이트 크기가 충분히 작으면 루프 종료
					terminateLoop = true;
			}

			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public float UpdateValueFunction(int gameStateKey)
		{
			GameState gameState = new GameState(gameStateKey); // 주어진 게임 상태 키에 대해 게임 상태 생성

			if (gameState.isFinalState()) // 게임 종료 상태이면 함수값 0 반환
				return 0.0f;

			List<float> actionExpectationList = new List<float>();

			for (int i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++) // 1부터 9까지의 모든 행동에 대해
			{
				if (gameState.IsValidMove(i)) // 이 행동이 올바른 행동이면
				{
					GameState nextState = gameState.GetNextState(i); // 행동을 통해 전이해 간 다음 상태 구성
					float reward = nextState.GetReward(); // 다음 상태에서의 보상값 확인

					float actionExpectation = reward + DiscountFactor * StateValueFunction[nextState.BoardStateKey]; // 행동 가치 함수값 계산

					actionExpectationList.Add(actionExpectation); // 계산된 가치 함수값을 저장
				}
			}

			if (actionExpectationList.Count > 0) // 루프 종료 후 반환할 가치 함수값 선택
			{
				if (gameState.NextTurn == 1) // 흑돌이 둘 차례이면 저장된 가치 함수값 중 최대값 반환
					return actionExpectationList.Max();
				else if (gameState.NextTurn == 2) // 백돌이 둘 차례이면 저장된 가치 함수값 중 최소값 반환
					return actionExpectationList.Min();
			}
			return 0.0f;
		}

		public int GetNextMove(int boardStateKey)
		{
			// 주어진 게임 상태에 대해서 선택할 수 있는 행동 후보값을 구한 후,
			IEnumerable<int> actionCandidates = GetNextMoveCandidate(boardStateKey);

			if (actionCandidates.Count() == 0)
				return 0;

			// 그 중 한 값을 랜덤하게 선택해서 반환
			return actionCandidates.ElementAt(Utilities.random.Next(0, actionCandidates.Count()));
		}

		public IEnumerable<int> GetNextMoveCandidate(int boardStateKey)
		{
			float selectedExpectation = 0.0f;

			GameState gameState = new GameState(boardStateKey); // 주어진 상태에 대한 게임 상태 생성
			Dictionary<int, float> actionCandidateDictionary = new Dictionary<int, float>();

			for (int i = GameParameters.ActionMinIndex; i <= GameParameters.ActionMaxIndex; i++) // 1부터 9까지의 모든 행동에 대해
			{
				if (gameState.IsValidMove(i)) // 이 행동에 이 상태에 적용 가능한 올바른 행동인 경우
				{
					GameState nextState = gameState.GetNextState(i); //그 행동을 통해 전이해가는 상태를 구하고
					float reward = nextState.GetReward(); // 그 전이해 간 상태에서의 보상값

					float actionExpectation = reward + DiscountFactor * StateValueFunction[nextState.BoardStateKey]; // 행동 가치 함수값 계산

					actionCandidateDictionary.Add(i, actionExpectation); // 행동과 그 행동에 대한 행동 가치 함수값을 저장
				}
			}

			if (actionCandidateDictionary.Count == 0)
				return new List<int>();

			if (gameState.NextTurn == 1) // 흑돌 차례인 경우 저장된 행동 가치 함수값 중 최대값을 선택
			{
				selectedExpectation = actionCandidateDictionary.Select(e => e.Value).Max();
			}
			else if (gameState.NextTurn == 2) // 백돌 차례인 경우 저장된 행동 가치 함수값 중 최소값 선택
			{
				selectedExpectation = actionCandidateDictionary.Select(e => e.Value).Min();
			}

			// 선택한 가치 함수값을 가지는 행동들을 모두 모아서 반환
			return actionCandidateDictionary.Where(e => e.Value == selectedExpectation).Select(e => e.Key);
		}

		public void SaveStateValueFunction()
		{
			// 가치 함수 저장
			// Json Serilizer 적용 후 text 형태로 저장
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;

			string stateValueFunctionInJson = JsonConvert.SerializeObject(StateValueFunction, settings);

			File.WriteAllText(stateValueFunctionFilePath, stateValueFunctionInJson);

			Console.Clear();
			Console.WriteLine($"가치 함수가 파일 {stateValueFunctionFilePath}에 저장되었습니다.");
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		public void LoadStateValueFunction()
		{
			// 가치 함수 로드
			// 텍스트 형태로 읽어온 후 Json Deserialize 적용
			if (File.Exists(stateValueFunctionFilePath))
			{
				string stateValueFunctionInJson = File.ReadAllText(stateValueFunctionFilePath);

				JsonSerializerSettings settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;

				StateValueFunction = JsonConvert.DeserializeObject<Dictionary<int, float>>(stateValueFunctionInJson, settings);

				Console.Clear();
				Console.WriteLine($"가치 함수가 파일 {stateValueFunctionFilePath}에서 로드되었습니다.");
			}
			else
			{
				Console.Clear();
				Console.WriteLine($"가치 함수 파일 {stateValueFunctionFilePath}이 존재하지 않습니다.");
			}
			Console.WriteLine(Environment.NewLine);
			Console.Write("아무 키나 누르세요:");
			Console.ReadLine();
		}

		private void StateCountReset()
		{
			num00 = 0;
			num10 = 0;
			num11 = 0;
			num21 = 0;
			num22 = 0;
			num32 = 0;
			num33 = 0;
			num43 = 0;
			num44 = 0;
		}
	}
}
