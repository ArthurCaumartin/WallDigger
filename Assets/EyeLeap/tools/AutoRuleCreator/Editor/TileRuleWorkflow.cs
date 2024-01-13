using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NelowGames;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEditor.VersionControl;
using UnityEditor.U2D.Sprites;
using System.IO;

namespace NelowGames {
    public class TileRuleWorkflow : ScriptableWizard {

        public static Texture2D texture;
        public static string resultLocalPath;
        public static string path;
        
        static int halfWidth, halfHeight;
        static int firstHalfWidth, firstHalfHeight;
        static int secondHalfWidth, secondHalfHeight;
        static int cellWidth, cellHeight;
        static Texture2D result;
        static Color[] pixels;

        public Vector2Int _center = Vector2Int.zero;
        public static Vector2Int pivotOffset = Vector2Int.zero;
        public Alignment pivot_anchor = Alignment.Center;
        public enum Alignment {
            Center,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }


        [MenuItem("Assets/Convert to Tileset  (automatic)")]
        static void Automatic() {
            texture = Selection.activeObject as Texture2D;
            path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if(!importer.isReadable) {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
            
            
            // reimport
            // AssetDatabase.ImportAsset(instance.path, ImportAssetOptions.ForceUpdate);

            resultLocalPath = path.Substring(0, path.LastIndexOf('/')+1) + texture.name + "_packed" + ".png";

            
            Alignment pivot_anchor = Alignment.Center;
            if(PlayerPrefs.HasKey("TileRuleWorkflow_pivotOffset_x")) {
                pivotOffset.x = PlayerPrefs.GetInt("TileRuleWorkflow_pivotOffset_x");
                pivotOffset.y = PlayerPrefs.GetInt("TileRuleWorkflow_pivotOffset_y");
                pivot_anchor = (Alignment)PlayerPrefs.GetInt("TileRuleWorkflow_pivot_anchor");
            } else {
                pivotOffset.x = texture.width / 10;
                pivotOffset.y = texture.height / 2;
                pivot_anchor = Alignment.TopLeft;
            }
            
            BakeCellDatas();


            Vector2Int _center = pivotOffset;
            switch(pivot_anchor) {
                case Alignment.BottomLeft:
                    pivotOffset = _center;
                    break;
                case Alignment.BottomRight:
                    pivotOffset = new Vector2Int(cellWidth - _center.x, _center.y);
                    break;
                case Alignment.TopLeft:
                    pivotOffset = new Vector2Int(_center.x, cellHeight - _center.y);
                    break;
                case Alignment.TopRight:
                    pivotOffset = new Vector2Int(cellWidth - _center.x, cellHeight - _center.y);
                    break;
                case Alignment.Center:
                    pivotOffset = new Vector2Int(cellWidth/2, cellHeight/2)  + _center;
                    break;
                default:
                    pivotOffset = _center;
                    break;
            }

            BakeCopySizes();

            string resultPath = CreateTileset();

            // QuickToCompleteWindow instance = ScriptableWizard.DisplayWizard<QuickToCompleteWindow>("Create Set", "Create", "Apply");
            // instance.Close();

            ConvertToRuleTile( AssetDatabase.LoadAssetAtPath(resultPath, typeof(Texture2D)) as Texture2D, out RuleTile tileset);

            #if NThemes
            ScriptableTheme theme = FindThemeRecursiveUp(path);
            if(theme != null) {
                // theme.tileset = AssetDatabase.LoadAssetAtPath<Tileset>(resultPath);
                theme.replaceWithTile = tileset;
                EditorUtility.SetDirty(theme);
                AssetDatabase.SaveAssets();
                UseInMap(theme);
            }
            #endif
        }
        


        [MenuItem("Assets/Convert to Tileset (with options)")]
        static void CreateWizard() {
            texture = Selection.activeObject as Texture2D;
            path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if(!importer.isReadable) {
                importer.isReadable = true;
                importer.SaveAndReimport();
                // reimport
                // AssetDatabase.ImportAsset(instance.path, ImportAssetOptions.ForceUpdate);
            }

            Debug.Log("converting... " + path);
            
            resultLocalPath = path.Substring(0, path.LastIndexOf('/')+1) + texture.name + "_packed" + ".png";

            TileRuleWorkflow instance = ScriptableWizard.DisplayWizard<TileRuleWorkflow>("Create Tileset", "Create", "Center");
            
            if(PlayerPrefs.HasKey("TileRuleWorkflow_pivotOffset_x")) {
                instance._center.x = PlayerPrefs.GetInt("TileRuleWorkflow_pivotOffset_x");
                instance._center.y = PlayerPrefs.GetInt("TileRuleWorkflow_pivotOffset_y");
                instance.pivot_anchor = (Alignment)PlayerPrefs.GetInt("TileRuleWorkflow_pivot_anchor");
            } else {
                instance._center.x = texture.width / 10;
                instance._center.y = instance._center.x;
                instance.pivot_anchor = Alignment.TopLeft;
            }
        }

        void OnWizardOtherButton() {
            _center = new Vector2Int(texture.width / 10, texture.height / 2);
        }

        void OnWizardCreate() {
            PlayerPrefs.SetInt("TileRuleWorkflow_pivotOffset_x", _center.x);
            PlayerPrefs.SetInt("TileRuleWorkflow_pivotOffset_y", _center.y);
            PlayerPrefs.SetInt("TileRuleWorkflow_pivot_anchor", (int)pivot_anchor);

            BakeCellDatas();
            switch(pivot_anchor) {
                case Alignment.BottomLeft:
                    pivotOffset = _center;
                    break;
                case Alignment.BottomRight:
                    pivotOffset = new Vector2Int(cellWidth - _center.x, _center.y);
                    break;
                case Alignment.TopLeft:
                    pivotOffset = new Vector2Int(_center.x, cellHeight - _center.y);
                    break;
                case Alignment.TopRight:
                    pivotOffset = new Vector2Int(cellWidth - _center.x, cellHeight - _center.y);
                    break;
                case Alignment.Center:
                    pivotOffset = new Vector2Int(cellWidth/2, cellHeight/2)  + _center;
                    break;
                default:
                    pivotOffset = _center;
                    break;
            }
            BakeCopySizes();
            
            string resultPath = CreateTileset();
            ConvertToRuleTile( AssetDatabase.LoadAssetAtPath(resultPath, typeof(Texture2D))  as Texture2D, out RuleTile tileset);
            
            #if NThemes
            ScriptableTheme theme = FindThemeRecursiveUp(path);
            if(theme != null) {
                // theme.tileset = AssetDatabase.LoadAssetAtPath<Tileset>(resultPath);
                theme.replaceWithTile = tileset;
                EditorUtility.SetDirty(theme);
                AssetDatabase.SaveAssets();
                UseInMap(theme);
            }
            #endif
        }


        // [MenuItem("Assets/Tileset to Rule Tile")]
        // static void ConvertToRuleTile() {
        //     ConvertToRuleTile(Selection.activeObject as Texture2D);
        // }
            
        static void ConvertToRuleTile(Texture2D TileMap, out RuleTile _new) {
            if(TileMap == null) {
                Debug.LogError("Incorrect selection");
            }
            _new = null;
            RuleTile RuleTileTemplate = Resources.Load("AutoRuleTile_default") as RuleTile;
            if (RuleTileTemplate == null) {
                Debug.LogError("template not found");
                return;
            } 
            // RuleTileTemplate = RuleTileTemplate_Default;

            string path = AssetDatabase.GetAssetPath(TileMap);

            
            // TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            // // hack to clear the metadatas.
            // importer.spriteImportMode = SpriteImportMode.Single;
            // importer.SaveAndReimport();
            // importer.spriteImportMode = SpriteImportMode.Multiple;
            
            // //? pivot, fhalf width from the top
            // Vector2 pivot = new Vector2( (float)pivotOffset.x / cellWidth, (float)pivotOffset.y / cellHeight );
            // int spriteAlignment = (int)SpriteAlignment.Custom;
            
            // // cut in 7x7
            // int cellIndex = 0;
            // cellWidth = TileMap.width / 7;
            // cellHeight = TileMap.height / 7;
            // List<SpriteMetaData> metas = new List<SpriteMetaData>();
            // for(int y = 6; y >= 0; y--) {
            //     for(int x = 0; x < 7; x++) {
            //         SpriteMetaData meta = new SpriteMetaData();
            //         meta.rect = new Rect(x*cellWidth, y*cellHeight, cellWidth, cellHeight);
            //         // meta.name = texture.name + "_" + x + "_" + y;
            //         if(cellIndex < 10)
            //             meta.name = TileMap.name + "_0" + cellIndex;
            //         else
            //             meta.name = TileMap.name + "_" + cellIndex;
                        
            //         meta.alignment = spriteAlignment;
            //         meta.pivot = pivot;

            //         metas.Add(meta);
            //         cellIndex++;
            //     }
            // }
            // importer.spritesheet = metas.ToArray();
            // importer.SaveAndReimport();
            

            // Replace this Asset with the new one.
            // Debug.Log("path: " + path);
            path = path.Substring(0, path.LastIndexOf(".")) + "_tile.asset";

            _new = AssetDatabase.LoadAssetAtPath(path, typeof(RuleTile)) as RuleTile;

            Debug.Log("converting " + TileMap.name + " to RuleTile at " + path + ", exists: " + (_new != null) + "");

            if(_new != null) {
                EditorUtility.CopySerialized(RuleTileTemplate, _new);
                
                // _new.name = TileMap.name + "_tile";
                _new.name = path.Substring( path.LastIndexOf("/") + 1 );
                _new.name = _new.name.Substring(0, _new.name.LastIndexOf("."));
                // just change the values of the existing asset
                
                // SerializedObject obk = new SerializedObject(_new);
                // obk.FindProperty("m_Name").stringValue = _new.name;

                // Get all the sprites in the Texture2D file (TileMap)
                string spriteSheet = AssetDatabase.GetAssetPath(TileMap);
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet)
                    .OfType<Sprite>().OrderBy( (x) => { return x.name; } ).ToArray();

                if (sprites.Length != RuleTileTemplate.m_TilingRules.Count) {
                    Debug.LogWarning("The Tilemap doesn't have the same number of sprites than the Rule Tile template has rules.");
                }

                // Set all the sprites of the TileMap.
                for (int i = 0; i < RuleTileTemplate.m_TilingRules.Count; i++) {
                    _new.m_TilingRules[i].m_Sprites[0] = sprites[i];
                    // Debug.Log("sprite: " + sprites[i].name + ", index : " + i);
                    _new.m_DefaultSprite = sprites[24];
                }

                AssetDatabase.SaveAssets();
            } else {
                // Make a copy of the Rule Tile Template from a new asset.
                _new = CreateInstance<RuleTile>();
                EditorUtility.CopySerialized(RuleTileTemplate, _new);
                
                // _new.name = TileMap.name + "_tile";
                _new.name = path.Substring( path.LastIndexOf("/") + 1 );
                _new.name = _new.name.Substring(0, _new.name.LastIndexOf("."));
                //Set serialzed meta name
                
                // SerializedObject obk = new SerializedObject(_new);
                // obk.FindProperty("m_Name").stringValue = _new.name;


                // Get all the sprites in the Texture2D file (TileMap)
                string spriteSheet = AssetDatabase.GetAssetPath(TileMap);
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet)
                    .OfType<Sprite>().ToArray();

                if (sprites.Length != RuleTileTemplate.m_TilingRules.Count) {
                    Debug.LogWarning("The Tilemap doesn't have the same number of sprites than the Rule Tile template has rules.");
                }

                // Set all the sprites of the TileMap.
                for (int i = 0; i < RuleTileTemplate.m_TilingRules.Count; i++) {
                    _new.m_TilingRules[i].m_Sprites[0] = sprites[i];
                    _new.m_DefaultSprite = sprites[24];
                }
                AssetDatabase.CreateAsset(_new, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log("Asset created as: " + _new.name);
        }
        
        [MenuItem("Assets/Convert to Tileset (with automatic)", true)]
        private static bool NewMenuOptionValidation1() {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/Convert to Tileset (with options)", true)]
        private static bool NewMenuOptionValidation2() {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/Convert to Rule Tile", true)]
        private static bool ConvertToRuleTileValidation() {
            return Selection.activeObject is Texture2D;
        }

        static int MapIN(ref int x, ref int y, out int height, out int width) {
            // int height = halfWidth;

            // 2 rows, 5 columns

            if(y == 0) {
                height = firstHalfHeight;
            }
            else {
                y = firstHalfHeight;
                height = secondHalfHeight;
            }

            // x = x*halfWidth;
            if(x % 2 == 0) { // left
                x = x*halfWidth;
                width = firstHalfWidth;
            }
            else {
                x = (x-1)*halfWidth + firstHalfWidth;
                width = secondHalfWidth;
            }

            return height;
        }
        static int MapOUT(ref int x, ref int y, out int height, out int width) {
            // height = halfWidth;

            if(y % 2 == 0) { // bottom
                y = y*halfHeight;
                height = firstHalfHeight;
            }
            else {
                y = (y-1)*halfHeight + firstHalfHeight;
                height = secondHalfHeight;
            }
            
            // x = x*halfWidth;
            if(x % 2 == 0) { // left
                x = x*halfWidth;
                width = firstHalfWidth;
            }
            else {
                x = (x-1)*halfWidth + firstHalfWidth;
                width = secondHalfWidth;
            }

            return height;
        }
        
        static void Copy( int x, int y ) {
            MapIN(ref x, ref y, out int height, out int width);
            try {
                pixels = texture.GetPixels(x, y, width, height);
            } catch(System.Exception e) {
                Debug.LogError("copy " + x + ", " + y + " of " + texture.width + "x" + texture.height);
                Debug.LogError("error copying " + texture);
                Debug.LogError("copying " + x + ", " + y + " : " + width + "x" + height + "");
                Debug.LogError(e);
                throw e;
            }
        }
        static void Paste( int x, int y ) {
            MapOUT(ref x, ref y, out int height, out int width);
            try {
                result.SetPixels(x, y, width, height, pixels);
            } catch(System.Exception e) {
                Debug.LogError("error pasting to " + result);
                Debug.LogError("pasting " + x + ", " + y + " : " + width + "x" + height + "");
                Debug.LogError(e);
                throw e;
            }
        }

        static void BakeCellDatas() {
            cellWidth = texture.width / 5;
            cellHeight = texture.height;
            halfWidth = cellWidth / 2;
            halfHeight = cellHeight / 2;
        }

        static void BakeCopySizes() {
            firstHalfWidth = pivotOffset.x;
            firstHalfHeight = pivotOffset.y;
            secondHalfWidth = cellWidth - pivotOffset.x;
            secondHalfHeight = cellHeight - pivotOffset.y;

            Debug.Log("cell size: " + cellWidth + "x" + cellHeight + ", pivot: " + pivotOffset.x + ", " + pivotOffset.y);
            Debug.Log("first half: " + firstHalfWidth + "x" + firstHalfHeight + ", second half: " + secondHalfWidth + "x" + secondHalfHeight + ".");
        }



        static string CreateTileset() {
		    result = new Texture2D(cellWidth * 7, cellHeight * 7, texture.format, false);

            // Copy pixels by corresponding chunks
            #region Corners
            //! connected to bottom, right
            Copy(8, 1);
            Paste(0, 13);
            Paste(0, 7);
            Paste(6, 13);
            Paste(6, 7);
            Paste(8, 5);
            
            
            //! connected to bottom, left
            Copy(9, 1);
            Paste(5, 13);
            Paste(7, 13);
            Paste(5, 7);
            Paste(7, 7);
            Paste(13, 5);
            
            // //! connected to top, right
            Copy(8, 0);
            Paste(0, 6);
            Paste(6, 6);
            Paste(0, 8);
            Paste(6, 8);
            Paste(8, 0);

            //! connected to top, left
            Copy(9, 0);
            Paste(5, 6);
            Paste(7, 6);
            Paste(5, 8);
            Paste(7, 8);
            Paste(13, 0);
            #endregion
            
            #region Vertical Edges 
            //! connected to top, bottom, right
            Copy(6, 1); // top part
            Paste(0, 11);
            Paste(0, 9);
            Paste(6, 11);
            Paste(6, 9);
            Paste(0, 5);
            Paste(0, 3);
            Paste(8, 3);
            Paste(8, 1);
            
            Copy(6, 0); // bottom part
            Paste(0, 12);
            Paste(0, 10);
            Paste(6, 12);
            Paste(6, 10);
            Paste(0, 4);
            Paste(0, 2);
            Paste(8, 4);
            Paste(8, 2);
            
            //! connected to top, bottom, left
            Copy(7, 1); // top part
            Paste(5, 11);
            Paste(5, 9);
            Paste(7, 11);
            Paste(7, 9);
            Paste(3, 5);
            Paste(3, 3);
            Paste(13, 3);
            Paste(13, 1);
            
            Copy(7, 0); // bottom part
            Paste(5, 12);
            Paste(5, 10);
            Paste(7, 12);
            Paste(7, 10);
            Paste(3, 4);
            Paste(3, 2);
            Paste(13, 4);
            Paste(13, 2);
            #endregion

            #region Horizontal Edges 
            //! connected to bottom, right, left
            Copy(4, 1); // left part
            Paste(2, 13);
            Paste(2, 7);
            Paste(4, 13);
            Paste(4, 7);

            Paste(4, 5);
            Paste(6, 5);
            Paste(10, 5);
            Paste(12, 5);
            
            Copy(5, 1); // right part
            Paste(1, 13);
            Paste(1, 7);
            Paste(3, 13);
            Paste(3, 7);

            Paste(5, 5);
            Paste(7, 5);
            Paste(9, 5);
            Paste(11, 5);
            
            //! connected to top, right, left
            Copy(4, 0); // left part
            Paste(2, 8);
            Paste(2, 6);
            Paste(4, 8);
            Paste(4, 6);

            Paste(4, 2);
            Paste(6, 2);
            Paste(10, 0);
            Paste(12, 0);
            
            Copy(5, 0); // right part
            Paste(1, 8);
            Paste(1, 6);
            Paste(3, 8);
            Paste(3, 6);

            Paste(5, 2);
            Paste(7, 2);
            Paste(9, 0);
            Paste(11, 0);
            #endregion

            #region Angles
            Copy(2, 1); // top left dir
            Paste(2, 3);
            Paste(6, 1);
            Paste(6, 3);
            Paste(8, 9);
            Paste(10, 1);
            Paste(10, 3);
            Paste(10, 7);
            Paste(10, 9);
            Paste(10, 11);
            Paste(12, 1);
            Paste(12, 3);
            Paste(12, 11);
            Paste(12, 13);

            Copy(3, 1); // top right dir
            Paste(1, 3);
            Paste(5, 1);
            Paste(5, 3);
            Paste(9, 1);
            Paste(9, 3);
            Paste(9, 7);
            Paste(9, 9);
            Paste(9, 11);
            Paste(11, 1);
            Paste(11, 3);
            Paste(11, 7);
            Paste(13, 9);
            Paste(13, 13);

            Copy(2, 0); // bottom left dir
            Paste(2, 4);
            Paste(4, 0);
            Paste(6, 4);
            Paste(8, 6);
            Paste(10, 2);
            Paste(10, 4);
            Paste(10, 6);
            Paste(10, 8);
            Paste(10, 12);
            Paste(12, 2);
            Paste(12, 4);
            Paste(12, 6);
            Paste(12, 10);

            Copy(3, 0); // bottom right dir
            Paste(1, 4);
            Paste(5, 4);
            Paste(7, 0);
            Paste(9, 2);
            Paste(9, 4);
            Paste(9, 6);
            Paste(9, 8);
            Paste(9, 12);
            Paste(11, 2);
            Paste(11, 4);
            Paste(11, 8);
            Paste(13, 6);
            Paste(13, 8);
            #endregion
            
            #region Plain
            Copy(0, 1); // top left block
            Paste(0, 1);
            Paste(2, 1);

            Paste(2, 5);
            Paste(2, 9);
            Paste(2, 11);
            Paste(4, 1);
            Paste(4, 3);
            Paste(4, 9);
            Paste(4, 11);
            Paste(8, 7);
            Paste(8, 11);
            Paste(8, 13);
            Paste(10, 13);
            Paste(12, 7);
            Paste(12, 9);
            
            Copy(1, 1); // top right block
            Paste(1, 1);
            Paste(3, 1);

            Paste(1, 5);
            Paste(1, 9);
            Paste(1, 11);
            Paste(3, 9);
            Paste(3, 11);
            Paste(7, 1);
            Paste(7, 3);
            Paste(9, 13);
            Paste(11, 9);
            Paste(11, 11);
            Paste(11, 13);
            Paste(13, 7);
            Paste(13, 11);
            
            Copy(0, 0); // bottom left block
            Paste(0, 0);
            Paste(2, 0);

            Paste(2, 2);
            // Paste(2, 8);
            Paste(2, 10);
            Paste(2, 12);
            Paste(4, 4);
            // Paste(4, 8);
            Paste(4, 10);
            Paste(4, 12);
            Paste(6, 0);
            Paste(8, 8);
            Paste(8, 10);
            Paste(8, 12);
            Paste(10, 10);
            Paste(12, 8);
            Paste(12, 12);

            Copy(1, 0); // bottom right block
            Paste(1, 0);
            Paste(3, 0);

            Paste(1, 2);
            Paste(1, 10);
            Paste(1, 12);
            Paste(3, 10);
            Paste(3, 12);
            Paste(5, 0);
            Paste(7, 4);
            Paste(9, 10);
            Paste(11, 6);
            Paste(11, 10);
            Paste(11, 12);
            Paste(13, 10);
            Paste(13, 12);

            #endregion
            
            result.Apply();
            byte[] bytes = result.EncodeToPNG();
            string filename = Application.dataPath.Substring(0, Application.dataPath.Length - 6);

            if(!filename.EndsWith("/")) filename += "/";
            filename += resultLocalPath;

            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Saved at : {0}", filename));
			DestroyImmediate(result);

            AssetDatabase.Refresh();
            
            TextureImporter importer = AssetImporter.GetAtPath(resultLocalPath) as TextureImporter;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = texture.filterMode;
            
            importer.spritePixelsPerUnit = texture.width/5;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.spriteBorder = new Vector4(0, 0, 0, 0);
            importer.mipmapEnabled = false;

            //? pivot, fhalf width from the top
            // float k = cellWidth / (float)cellHeight;
            // Vector2 pivot = new Vector2(.5f, .5f.Remap(0, 1, 1, 1-k) );
            Vector2 pivot = new Vector2( (float)pivotOffset.x / cellWidth, (float)pivotOffset.y / cellHeight );
            int spriteAlignment = (int)SpriteAlignment.Custom;
            
            // cut in 7x7
            // int cellIndex = 0;
            // List<SpriteMetaData> metas = new List<SpriteMetaData>();
            // for(int y = 6; y >= 0; y--) {
            //     for(int x = 0; x < 7; x++) {
            //         SpriteMetaData meta = new SpriteMetaData();
            //         meta.rect = new Rect(x*cellWidth, y*cellHeight, cellWidth, cellHeight);
            //         // meta.name = texture.name + "_" + x + "_" + y;
            //         if(cellIndex < 10)
            //             meta.name = texture.name + "_0" + cellIndex;
            //         else
            //             meta.name = texture.name + "_" + cellIndex;

            //         meta.alignment = spriteAlignment;
            //         meta.pivot = pivot;
            //         meta.border = new Vector4(0, 0, 0, 0);

            //         metas.Add(meta);
            //         cellIndex++;
            //     }
            // }
            importer.SaveAndReimport();
            
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();
            // SpriteRect[] spriteRects = dataProvider.GetSpriteRects();//if you don't have the initial sprite rects, just create them manually, this example just changes the pivot points
            SpriteRect[] spriteRects = dataProvider.GetSpriteRects();
            bool hasRect = spriteRects.Length == 7*7;
            if(hasRect) {
                Debug.Log(texture + " has rects");
            } else {
                Debug.Log(texture + " has no rects, generating new guiids");
                spriteRects = new SpriteRect[7*7];
            }
            
            for(int y = 6; y >= 0; y--) {
                for(int x = 0; x < 7; x++) {
                    SpriteRect r = new SpriteRect();
                    r.rect = new Rect(x*cellWidth, y*cellHeight, cellWidth, cellHeight);
                    r.pivot = pivot;
                    r.alignment = (UnityEngine.SpriteAlignment) spriteAlignment;
                    r.border = new Vector4(0, 0, 0, 0);

                    int i = x + (6-y)*7;
                    if(i < 10)
                        r.name = texture.name + "_0" + i;
                    else
                        r.name = texture.name + "_" + i;

                    // if(!hasRect) 
                    GenerateName(ref r);
                    // Debug.Log("name: " + r.name + ", guid: " + r.spriteID);

                    i = x + y*7;
                    spriteRects[i] = r;
                }
            }

            dataProvider.SetSpriteRects(spriteRects);
            dataProvider.Apply();

           //! 
            // Additional step for Unity 2021.2 and newer
            var nameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var pairs = nameFileIdDataProvider.GetNameFileIdPairs();
            foreach (var pair in pairs) {
                var spriteRect = System.Array.Find(spriteRects, x => x.spriteID == pair.GetFileGUID());
                if(pair != null && spriteRect != null)
                    pair.name = spriteRect.name;
            }
        
            nameFileIdDataProvider.SetNameFileIdPairs(pairs);
           //!

            // if(!hasRect) {
            //     GenerateSpriteNames(dataProvider, ref SpriteRects);
            // }

            dataProvider.Apply();
            
            importer.SaveAndReimport();



            AssetDatabase.Refresh();

            return resultLocalPath;
        }
        
        //! https://forum.unity.com/threads/how-to-change-individual-spritesheet-in-textureimporter-object-without-regenerating-all-spriteids.631252/#post-7381619
        static void GenerateSpriteNames(ISpriteEditorDataProvider dataProvider) {
            var spriteRects = dataProvider.GetSpriteRects();
            for (var i = 0; i < spriteRects.Length; ++i) {
                spriteRects[i].spriteID = GUID.Generate();
                spriteRects[i].name =  GUID.Generate().ToString();
                Debug.Log("name: " + spriteRects[i].name + ", id: " + spriteRects[i].spriteID + ", guid: " + spriteRects[i].spriteID.ToString() + "");
            }
            dataProvider.SetSpriteRects(spriteRects);
        
            // Additional step for Unity 2021.2 and newer
            var nameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var pairs = nameFileIdDataProvider.GetNameFileIdPairs();
            foreach (var pair in pairs) {
                var spriteRect = System.Array.Find(spriteRects, x => x.spriteID == pair.GetFileGUID());
                pair.name = spriteRect.name;
            }
        
            nameFileIdDataProvider.SetNameFileIdPairs(pairs);
        }

        static void GenerateName(ref SpriteRect r) {
            r.spriteID = GUID.Generate();
            // r.name =  GUID.Generate().ToString();
        }
        
        // static string GetRandomName() {
        //     return GUID.Generate().ToString();
        // }

        
        [MenuItem("Assets/Use in current MAP")]
        static void UseInCurrentMap() {
            // TileBase tileBase = Selection.activeObject as TileBase;
            // if(tileBase == null) {
            //     Debug.LogError("No Tile selected");
            //     return;
            // }

            // Map map = FindObjectOfType<Map>();
            // if(map == null || map.groundTileMap == null) {
            //     Debug.LogError("No Tilemap found in scene");
            //     return;
            // }

            // map.groundTileMap.CompressBounds();
            // BoundsInt bounds = map.groundTileMap.cellBounds;
            // TileBase[] allTiles = map.groundTileMap.GetTilesBlock(bounds);
            // for(int x = 0; x < bounds.size.x; x++) {
            //     for(int y = 0; y < bounds.size.y; y++) {
            //         TileBase tile = allTiles[x + y * bounds.size.x];
            //         if(tile != null) {
            //             map.groundTileMap.SetTile(new Vector3Int(x + bounds.xMin, y + bounds.yMin, 0), tileBase);
            //         }
            //     }
            // }
            #if NThemes
            UseInMap(Selection.activeObject as ScriptableTheme);
            #endif
        }
        #if NThemes
        static void UseInMap(ScriptableTheme theme) {
            if(theme == null) {
                Debug.LogError("No Theme selected");
                return;
            }
            GenericSettings settings = FindObjectOfType<GenericSettings>();
            if(settings == null) {
                Debug.LogError("No GenericSettings found in scene");
                return;
            }

            settings.SetTheme(theme);
            EditorUtility.SetDirty(settings);
        }
        [MenuItem("Assets/Use in current MAP", true)]
        private static bool UseInCurrentMapValidation() {
            // return Selection.activeObject is TileBase;
            return Selection.activeObject is ScriptableTheme;
        }


        static ScriptableTheme FindThemeRecursiveUp(string path) {
            // Debug.Log("checking dir : " + path);
            path = path.Substring(0, path.LastIndexOf("/"));
            path = path.Substring(0, path.LastIndexOf("/"));
            // Debug.Log("checking dir : " + path);
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.asset");
            foreach(FileInfo file in files) {
                // Debug.Log("checking " + path + "/" + file.Name);
                ScriptableTheme theme = AssetDatabase.LoadAssetAtPath<ScriptableTheme>(path + "/" + file.Name);
                if(theme != null) {
                    // Debug.Log("Theme found at " + path + "/" + file.Name + "");
                    return theme;
                }
            }
            // Debug.Log("Theme not found at " + path);
            return null;
        }
        #endif

    }
}