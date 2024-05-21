using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.AnimatedValues;

/**
 * The Game Manager Editor Script formats the public paramters for the Game Manager
 * To learn how to write Editor Scripts, check out the Unity Documentation.
 */
[CustomEditor(typeof(SF2X_GameManager))]

public class SF2X_GameManagerEditor : Editor
{
    #region  Definitions

    private int headerWidthCorrectionForScaling = 38;
    public string headerFlexibleStyle = "Box";
    private Texture2D header;
    public Color backgroundColorByDefault;
    private static GUIStyle boxExpanded;
    private Section sectionUserInterface;
    private Section sectionCharacter;
    private Section sectionNetwork;
    private Section sectionAudio;
    private Section sectionCineMachine;

    SerializedProperty SyncMode;
    SerializedProperty InputAsset;
    SerializedProperty gunShot;
    SerializedProperty reLoad;
    SerializedProperty footStep;
    SerializedProperty Landing;
    SerializedProperty Wounded;
    SerializedProperty firstshoulderOffset;
    SerializedProperty secondshoulderOffset;
    SerializedProperty thirdshoulderOffset;
    SerializedProperty healthstars;
    SerializedProperty loadedbullets;
    SerializedProperty unloadedbullets;
    SerializedProperty Kills;
    SerializedProperty crosshairs;
    SerializedProperty ammoprefab;
    SerializedProperty healthprefab;
    SerializedProperty healthbar;
    SerializedProperty helpcanvas;
    SerializedProperty infocanvas1;
    SerializedProperty infocanvas2;
    SerializedProperty killcanvas;
    SerializedProperty playerprefab;
    SerializedProperty colorArray;
    SerializedProperty waittoRespawn;

    #endregion

    #region  Section Definition
    public class Section
    {
        private const string KEY_STATE = "sections";

        public GUIContent name;
        public AnimBool state;
        private static GUIStyle btnToggleNormalOn;
        private static GUIStyle btnToggleNormalOff;

        public Section(string name, Texture2D icon, UnityAction repaint)
        {
            this.name = new GUIContent(string.Format(" {0}", name), icon);
            this.state = new AnimBool(this.GetState());
            this.state.speed = 3f;
            this.state.valueChanged.AddListener(repaint);
        }
        public void PaintSection()
        {
            GUIStyle buttonStyle = (this.state.target
                 ? GetToggleButtonNormalOn()
                 : GetToggleButtonNormalOff()
             );

            if (GUILayout.Button(this.name, buttonStyle))
            {
                this.state.target = !this.state.target;
                string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
                EditorPrefs.SetBool(key, this.state.target);
            }
        }
        private bool GetState()
        {
            string key = string.Format(KEY_STATE, this.name.text.GetHashCode());
            return EditorPrefs.GetBool(key, true);
        }

        public static GUIStyle GetToggleButtonNormalOn()
        {
            if (btnToggleNormalOn == null)
            {
                btnToggleNormalOn = new GUIStyle(GetToggleButtonNormalOff());
                btnToggleNormalOn.alignment = TextAnchor.MiddleLeft;
                btnToggleNormalOn.normal = btnToggleNormalOn.onNormal;
                btnToggleNormalOn.hover = btnToggleNormalOn.onHover;
                btnToggleNormalOn.active = btnToggleNormalOn.onActive;
                btnToggleNormalOn.focused = btnToggleNormalOn.onFocused;
                btnToggleNormalOn.richText = true;
                btnToggleNormalOff.fixedHeight = 24f;
                btnToggleNormalOn.margin.bottom = 2;
            }
            return btnToggleNormalOn;
        }
        public static GUIStyle GetToggleButtonNormalOff()
        {
            if (btnToggleNormalOff == null)
            {
                btnToggleNormalOff = new GUIStyle(GUI.skin.GetStyle("Button"));
                btnToggleNormalOff.alignment = TextAnchor.MiddleLeft;
                btnToggleNormalOff.richText = true;
                btnToggleNormalOff.fixedHeight = 24f;
                btnToggleNormalOff.margin.bottom = 2;
            }
            return btnToggleNormalOff;
        }
    }
    #endregion
   
    #region  Inspector Overides
    void OnEnable()
    {
        if (EditorGUIUtility.isProSkin)
        {
            string headerPathDark = Path.Combine("Assets", "Game", "Images", "logo-sfs-dark.png");
            header = AssetDatabase.LoadAssetAtPath<Texture2D>(headerPathDark);
        }
        else
        {
            string headerPathLight = Path.Combine("Assets", "Game", "Images", "logo-sfs-light.png");
            header = AssetDatabase.LoadAssetAtPath<Texture2D>(headerPathLight);
        }
        string iconModelPath1 = Path.Combine("Assets", "Game", "Images", "icons", "character.png");
        Texture2D iconModel1 = AssetDatabase.LoadAssetAtPath<Texture2D>(iconModelPath1);
        this.sectionCharacter = new Section("Character Settings", iconModel1, this.Repaint);

        string iconModelPath2 = Path.Combine("Assets", "Game", "Images", "icons", "camera.png");
        Texture2D iconModel2 = AssetDatabase.LoadAssetAtPath<Texture2D>(iconModelPath2);
        this.sectionCineMachine = new Section("CineMachine Settings", iconModel2, this.Repaint);

        string iconModelPath3 = Path.Combine("Assets", "Game", "Images", "icons", "uifunctions.png");
        Texture2D iconModel3 = AssetDatabase.LoadAssetAtPath<Texture2D>(iconModelPath3);
        this.sectionUserInterface = new Section("UI Settings", iconModel3, this.Repaint);

        string iconModelPath4 = Path.Combine("Assets", "Game", "Images", "icons", "audioclips.png");
        Texture2D iconModel4 = AssetDatabase.LoadAssetAtPath<Texture2D>(iconModelPath4);
        this.sectionAudio = new Section("Audio Settings", iconModel4, this.Repaint);

        string iconModelPath5 = Path.Combine("Assets", "Game", "Images", "icons", "network.png");
        Texture2D iconModel5 = AssetDatabase.LoadAssetAtPath<Texture2D>(iconModelPath5);
        this.sectionNetwork = new Section("Network/Player", iconModel5, this.Repaint);

        SyncMode = serializedObject.FindProperty("NetworkSyncMode");
        InputAsset = serializedObject.FindProperty("inputActionAsset");
        gunShot = serializedObject.FindProperty("gunshot");
        reLoad = serializedObject.FindProperty("reload");
        footStep = serializedObject.FindProperty("footstep");
        Landing = serializedObject.FindProperty("landing");
        Wounded = serializedObject.FindProperty("wounded");
        firstshoulderOffset = serializedObject.FindProperty("firstShoulderOffset");
        secondshoulderOffset = serializedObject.FindProperty("secondShoulderOffset");
        thirdshoulderOffset = serializedObject.FindProperty("thirdShoulderOffset");
        healthstars = serializedObject.FindProperty("healthStars");
        loadedbullets = serializedObject.FindProperty("loadedBullets");
        unloadedbullets = serializedObject.FindProperty("unloadedBullets");
        Kills = serializedObject.FindProperty("kills");
        crosshairs = serializedObject.FindProperty("crossHairs");
        ammoprefab = serializedObject.FindProperty("ammoPrefab");
        healthprefab = serializedObject.FindProperty("healthPrefab");
        healthbar = serializedObject.FindProperty("healthBar");
        helpcanvas = serializedObject.FindProperty("helpCanvas");
        infocanvas1 = serializedObject.FindProperty("infoCanvas1");
        infocanvas2 = serializedObject.FindProperty("infoCanvas2");
        killcanvas = serializedObject.FindProperty("killCanvas");
        playerprefab = serializedObject.FindProperty("playerPrefab");
        colorArray = serializedObject.FindProperty("colorarray");
        waittoRespawn = serializedObject.FindProperty("waitToRespawn");

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawEditorByDefaultWithHeaderAndHelpBox();
        EditorGUILayout.Space();
        this.PaintCharacter();
        this.PaintNetwork();
        this.PaintCineMachine();
        this.PaintUI();
        this.PaintAudio();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
    }

    public static GUIStyle GetBoxExpanded()
    {
        if (boxExpanded == null)
        {
            boxExpanded = new GUIStyle(EditorStyles.helpBox);
            boxExpanded.padding = new RectOffset(1, 1, 3, 3);
            boxExpanded.margin = new RectOffset(boxExpanded.margin.left, boxExpanded.margin.right, 0, 0);
        }
        return boxExpanded;
    }
    #endregion

    #region  Paint Sections
    private void PaintUI()
    {
        this.sectionUserInterface.PaintSection();
        using (var group = new EditorGUILayout.FadeGroupScope(this.sectionUserInterface.state.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.BeginVertical(GetBoxExpanded());
                EditorGUILayout.LabelField("User Interface", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(healthbar, new GUIContent("Health Bar Prefab"));
                EditorGUILayout.PropertyField(healthprefab, new GUIContent("Health Box Prefab"));
                EditorGUILayout.PropertyField(ammoprefab, new GUIContent("Ammo Box Prefab"));
                EditorGUILayout.PropertyField(helpcanvas, new GUIContent("Help Canvas"));
                EditorGUILayout.PropertyField(infocanvas2, new GUIContent("Item Info Canvas"));
                EditorGUILayout.PropertyField(killcanvas, new GUIContent("Kill Canvas"));
                EditorGUILayout.PropertyField(infocanvas1, new GUIContent("Player Info Canvas"));
                EditorGUI.indentLevel++; EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(healthstars, new GUIContent("Health Stars"));
                EditorGUILayout.PropertyField(loadedbullets, new GUIContent("Loaded Bullets"));
                EditorGUILayout.PropertyField(unloadedbullets, new GUIContent("Unloaded Bullets"));
                EditorGUILayout.PropertyField(Kills, new GUIContent("Kill Icons"));
                EditorGUILayout.Space(); EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(crosshairs, new GUIContent("Optional Crosshairs"));
                EditorGUI.indentLevel--; EditorGUI.indentLevel--; EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }
    }

    private void PaintCharacter()
    {
        this.sectionCharacter.PaintSection();
        using (var group = new EditorGUILayout.FadeGroupScope(this.sectionCharacter.state.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.BeginVertical(GetBoxExpanded());
                EditorGUILayout.LabelField("Character Prefabs", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(playerprefab, new GUIContent("Random Player Characters"));
                EditorGUILayout.PropertyField(colorArray, new GUIContent("Random Colors for Character"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(); 
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }
    }

    private void PaintCineMachine()
    {
        this.sectionCineMachine.PaintSection();
        using (var group = new EditorGUILayout.FadeGroupScope(this.sectionCineMachine.state.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.BeginVertical(GetBoxExpanded());
                EditorGUILayout.LabelField("Cinemachine Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(firstshoulderOffset, new GUIContent("1st Person Offset"));
                EditorGUILayout.PropertyField(secondshoulderOffset, new GUIContent("2nd Person Offset"));
                EditorGUILayout.PropertyField(thirdshoulderOffset, new GUIContent("3rd Person Offset"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(); 
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }
    }

    private void PaintAudio()
    {
        this.sectionAudio.PaintSection();
        using (var group = new EditorGUILayout.FadeGroupScope(this.sectionAudio.state.faded))
        {
           if (group.visible)
            {
                EditorGUILayout.BeginVertical(GetBoxExpanded());
                EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(gunShot, new GUIContent("Gunshot Sound"));
                EditorGUILayout.PropertyField(reLoad, new GUIContent("Reloading Sound"));
                EditorGUILayout.PropertyField(footStep, new GUIContent("Character Footsteps"));
                EditorGUILayout.PropertyField(Landing, new GUIContent("Character Landing"));
                EditorGUILayout.PropertyField(Wounded, new GUIContent("Character Wounded"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(); 
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }
    }


    private void PaintNetwork()
    {
        this.sectionNetwork.PaintSection();
        using (var group = new EditorGUILayout.FadeGroupScope(this.sectionNetwork.state.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.BeginVertical(GetBoxExpanded());
                EditorGUILayout.LabelField("Network Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(SyncMode, new GUIContent("Network Sync Mode"));
                EditorGUILayout.Space(); EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Player Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(InputAsset, new GUIContent("Input Action Asset"));
                EditorGUILayout.PropertyField(waittoRespawn, new GUIContent("Time (secs) to ReSpawn"));
                EditorGUILayout.Space(); EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }
    }
    #endregion

    #region  Draw Functions
    private void ArrayGUI(SerializedProperty property, string itemType, bool visible)
    {
        {
            EditorGUI.indentLevel++;
            SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
            EditorGUILayout.PropertyField(arraySizeProp);
            for (int i = 0; i < arraySizeProp.intValue; i++)
            {
                EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent(itemType + (i + 1).ToString()), true);

            }
            EditorGUI.indentLevel--;
        }
    }
    public void DrawEditorByDefaultWithHeaderAndHelpBox()
    {
        DrawHeaderFlexible(header, Color.clear);
        DrawHelpBox();
    }

    public void DrawHeaderFlexible(Texture2D header, Color backgroundColor)
    {
        if (header)
        {
            if (header.width + headerWidthCorrectionForScaling < EditorGUIUtility.currentViewWidth)
            {
                EditorGUILayout.BeginVertical(headerFlexibleStyle);
                DrawHeader(header);
                EditorGUILayout.EndVertical();
            }
            else
            {
                DrawHeaderIfScrollbar(header);
            }
        }
    }

    public void DrawHeaderIfScrollbar(Texture2D header)
    {
        EditorGUI.DrawTextureTransparent(
            GUILayoutUtility.GetRect(
            EditorGUIUtility.currentViewWidth - headerWidthCorrectionForScaling,
            header.height),
            header,
            ScaleMode.ScaleToFit);
    }

    public void DrawHeader(Texture2D header)
    {
        EditorGUI.DrawTextureTransparent(
            GUILayoutUtility.GetRect(
            header.width,
            header.height),
            header,
            ScaleMode.ScaleToFit);
    }


    public void DrawHelpBox()
    {
        LinkButton("https://www.smartfoxserver.com");
    }

    private void LinkButton(string url)
    {
        var style = GUI.skin.GetStyle("HelpBox");
        style.richText = true;
        style.alignment = TextAnchor.MiddleCenter;
        bool bClicked = GUILayout.Button("<b>Online Documentation can be found at https://www.smartfoxserver.com/</b>", style);
        var rect = GUILayoutUtility.GetLastRect();
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        if (bClicked)
            Application.OpenURL(url);
    }
    #endregion

}