using System;

namespace Reinforcement_Learning
{
	public enum QFunctionType
	{
		SARSA,
		QLEARNING
	}

	class MainProgram
	{
		public static StateValueFunctionManager ValueFunctionManager;
		public static SarsaManager SarsaValueFunctionManager;
		public static QLearningManager QLearningValueFunctionManager;
		public static GameManager GonuGameManager;

		static void Main(string[] args)
		{
			ValueFunctionManager = new StateValueFunctionManager();
			SarsaValueFunctionManager = new SarsaManager();
			QLearningValueFunctionManager = new QLearningManager();
			GonuGameManager = new GameManager();

			bool showMenu = true;

			while(showMenu)
			{
				showMenu = MainMenu();
			}
		}

		private static bool MainMenu()
		{
			Console.Clear();
			Console.WriteLine("원하는 동작을 선택하세요:");
			Console.WriteLine(Environment.NewLine);
			Console.WriteLine("1) 동적프로그래밍 진행");
			Console.WriteLine("2) 동적 프로그래밍 가치 함수 저장");
			Console.WriteLine("3) 동적 프로그래밍 가치 함수 읽어오기");
			Console.WriteLine("4) SARSA 진행");
			Console.WriteLine("5) SARSA 가치 함수 저장");
			Console.WriteLine("6) SARSA 가치 함수 읽어오기");
			Console.WriteLine("7) Q-러닝 진행");
			Console.WriteLine("8) Q-러닝 가치 함수 저장");
			Console.WriteLine("9) Q-러닝 가치 함수 읽어오기");
			Console.WriteLine("10) 게임 하기");
			Console.WriteLine("11) 프로그램 종료");
			Console.WriteLine(Environment.NewLine);
			Console.Write("동작 선택:");

			switch(Console.ReadLine())
			{
				case "1":
					ValueFunctionManager.UpdateByDynamicProgramming();
					return true;
				case "2":
					ValueFunctionManager.SaveStateValueFunction();
					return true;
				case "3":
					ValueFunctionManager.LoadStateValueFunction();
					return true;
				case "4":
					SarsaValueFunctionManager.UpdateBySarsa();
					return true;
				case "5":
					SarsaValueFunctionManager.SaveStateValueFunction();
					return true;
				case "6":
					SarsaValueFunctionManager.LoadStateValueFunction();
					return true;
				case "7":
					QLearningValueFunctionManager.UpdateByQLearning();
					return true;
				case "8":
					QLearningValueFunctionManager.SaveStateValueFunction();
					return true;
				case "9":
					QLearningValueFunctionManager.LoadStateValueFunction();
					return true;
				case "10":
					GonuGameManager.PlayGame();
					return true;
				case "11":
					return false;
				default:
					return true;
			}
		}
	}
}
