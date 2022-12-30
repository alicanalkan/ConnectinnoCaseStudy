using System;

namespace ConnectinnoGames.GameScripts
{
    /// <summary>
    /// Delegate actions for game functions callable 
    /// </summary>
    public static class ConnectinnoActions
    {
        public static Action OnRecipeCompleted;
        public static Action<BaseRecipe> OnRecipeStarted;
        public static Action OnReplayLevel;
        public static Action OnExtraSeconds;
        public static Action<int> OnAddCoin;
        public static Action<int> OnRemoveCoin;
        public static Action StartTimer;
        public static Action<IngredientType ,int> OnCorrectIngredientPlaced;
    }
}