using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace ChatVisual
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private int enumValue;
        private int enumValue2;

        private List<EChatEvent> chatEventList = new List<EChatEvent>();
        private List<string> LoadFileList = new List<string>();     

        private bool is_Expand = false;
        private bool is_LoadList = false;

        public void UpdateInspector(NodeView node)   
        {
            Clear();        // Delete all existing children

            is_Expand = false;
            is_LoadList = false;

            var container = new IMGUIContainer();
            container.onGUIHandler = () =>
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 20;
                GUILayout.Label($"{node.node.GetType().Name}", style);

                bool is_ChildExist = false;
                switch (node.node)
                {
                    case RootNode:
                        {
                            RootNode rootNode = node.node as RootNode;
                            if (rootNode.child != null) is_ChildExist = true;
                            GUILayout.Space(15);

                            rootNode.showName = EditorGUILayout.TextField("ShowName", rootNode.showName, EditorStyles.textArea);
                            GUILayout.Space(10);

                            // round
                            if (!is_LoadList)
                            {
                                LoadFileList = new List<string>(rootNode.loadFileNameList);
                                is_LoadList = true;
                            }
                            GUIStyle boxStyle = EditorStyles.helpBox;
                            GUILayout.BeginVertical(boxStyle);
                            is_Expand = EditorGUILayout.BeginFoldoutHeaderGroup(is_Expand, "Round List", menuAction: ShowHeaderContextMenu);
                            if (is_Expand)
                            {
                                for (int i = 0; i < LoadFileList.Count; ++i)
                                {
                                    LoadFileList[i] = EditorGUILayout.TextArea(LoadFileList[i]);
                                }
                                if (GUILayout.Button("Add Round"))
                                {
                                    LoadFileList.Add("");
                                }
                            }
                            rootNode.loadFileNameList = new List<string>(LoadFileList);
                            EditorGUILayout.EndFoldoutHeaderGroup();
                            GUILayout.EndVertical();
                        }
                        break;
                    case ChatNode:
                        {
                            GUILayout.Space(10);
                            ChatNode chatNode = node.node as ChatNode;
                            if (chatNode.childList.Count != 0) is_ChildExist = true;      

                            enumValue = (int)chatNode.state;
                            enumValue = GUILayout.Toolbar(enumValue, System.Enum.GetNames(typeof(EChatState))); 
                            chatNode.state = (EChatState)enumValue;
                            GUILayout.Space(5);

                            GUILayout.Label("Chat");
                            chatNode.text = EditorGUILayout.TextArea(chatNode.text, EditorStyles.textArea);
                            GUILayout.Space(10);

                            enumValue2 = (int)chatNode.face;
                            enumValue2 = GUILayout.Toolbar(enumValue2, System.Enum.GetNames(typeof(EFace)));     
                            chatNode.face = (EFace)enumValue2;
                            GUILayout.Space(10);

                            if (!is_LoadList)
                            {
                                chatEventList = new List<EChatEvent>(chatNode.textEvent);
                                is_LoadList = true;
                            }
                            GUIStyle boxStyle = EditorStyles.helpBox;
                            GUILayout.BeginVertical(boxStyle);
                            is_Expand = EditorGUILayout.BeginFoldoutHeaderGroup(is_Expand, "Chat Event List", menuAction: ShowHeaderContextMenu);
                            if (is_Expand)
                            {
                                for (int i = 0; i < chatEventList.Count; ++i)
                                {
                                    chatEventList[i] = (EChatEvent)EditorGUILayout.EnumPopup(chatEventList[i]);
                                }
                                if (GUILayout.Button("Add Chat Event"))
                                {
                                    chatEventList.Add(EChatEvent.Vibration);
                                }
                            }
                            chatNode.textEvent = new List<EChatEvent>(chatEventList);
                            EditorGUILayout.EndFoldoutHeaderGroup();
                            GUILayout.EndVertical();
                        }
                        break;
                    case AskNode:
                        {
                            GUILayout.Space(10);
                            AskNode askNode = node.node as AskNode;
                            if (askNode.child != null) is_ChildExist = true;

                            GUILayout.Label("Ask");
                            askNode.ask = EditorGUILayout.TextArea(askNode.ask, EditorStyles.textArea);

                            if (!is_LoadList)
                            {
                                chatEventList = new List<EChatEvent>(askNode.textEvent);
                                is_LoadList = true;
                            }
                            GUIStyle boxStyle = EditorStyles.helpBox;
                            GUILayout.BeginVertical(boxStyle);
                            is_Expand = EditorGUILayout.BeginFoldoutHeaderGroup(is_Expand, "Chat Event List", menuAction: ShowHeaderContextMenu);
                            if (is_Expand)
                            {
                                for (int i = 0; i < chatEventList.Count; ++i)
                                {
                                    chatEventList[i] = (EChatEvent)EditorGUILayout.EnumPopup(chatEventList[i]);
                                }
                                if (GUILayout.Button("Add Chat Event"))
                                {
                                    chatEventList.Add(EChatEvent.Vibration);
                                }
                            }
                            askNode.textEvent = new List<EChatEvent>(chatEventList);
                            EditorGUILayout.EndFoldoutHeaderGroup();
                            GUILayout.EndVertical();

                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.Toggle("is_UseThie", askNode.is_UseThis);
                        }
                        break;
                }

                GUILayout.Space(5);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("child", is_ChildExist);
                EditorGUI.EndDisabledGroup();
            };

            Add(container);
        }

        private void ShowHeaderContextMenu(Rect position)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Clear"), false, () =>
            {
                Debug.Log("Clear");
                chatEventList.Clear();
                LoadFileList.Clear();
            });
            menu.DropDown(position);
        } 
    }
}
