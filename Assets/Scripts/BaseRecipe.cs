using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ConnectinnoGames.GameScripts;
namespace ConnectinnoGames.GameScripts
{
    public class BaseRecipe : ScriptableObject
    {
        public string RecipieName;
        public List<RecipeIngredients> Ingredients;
        public RecipeDatabase myDatabase;
        public void Initialize(RecipeDatabase recipeDatabase)
        {
            myDatabase = recipeDatabase;
        }
        public void Rename()
        {
            name = RecipieName;
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
        }

    }
    [CustomEditor(typeof(BaseRecipe))]
    public class BaseRecipeEditor : Editor
    {
        private BaseRecipe baseRecipe;

        private void OnEnable()
        {
            if (baseRecipe == null)
            {
                baseRecipe = target as BaseRecipe;
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Set"))
            {
                baseRecipe.Rename();
            }

            base.OnInspectorGUI();
        }
    }
}
