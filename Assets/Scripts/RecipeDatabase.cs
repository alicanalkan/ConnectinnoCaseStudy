using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ConnectinnoGames.GameScripts
{
    [CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Connectionno/Recipies")]
    public class RecipeDatabase : ScriptableObject
    {
        public List<BaseRecipe> recipes = new List<BaseRecipe>();

        public void CreateNewRecipe()
        {
           
            var recipe = CreateInstance<BaseRecipe>();
            recipe.Initialize(this);
            recipes.Add(recipe);
          

            AssetDatabase.AddObjectToAsset(recipe, this);
            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(recipe);
        }
    }
    [CustomEditor(typeof(RecipeDatabase))]
    public class RecipeDatabaseEditor : Editor
    {
        private RecipeDatabase recipeDatabase;

        private void OnEnable()
        {
            if (recipeDatabase == null)
            {
                recipeDatabase = target as RecipeDatabase;
            }
        }

        public override void OnInspectorGUI()
        {
            // Make a button that calls a static method to open the editor window,
            // passing in the scriptable object information from which the button was pressed
            if (GUILayout.Button("Create New Recipe"))
            {
                recipeDatabase.CreateNewRecipe();
            }

            base.OnInspectorGUI();
        }
    }
}
