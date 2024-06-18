using ChatVisual;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChatHumanManager : UI_Reader
{
    public ChatContainer chatContainer;

    public float changeHumanTime = 1f;       // A time when humans change
    public float nextChatTime = 1f;         // when load next chat time
    private float currentTime = 0f;
    private bool is_ChatStart = false;

    private bool is_ask = false;

    private List<Node> nowNodes = new List<Node>();
    public string nowHumanName;        // Name of the human you're talking to
    public MemberProfile nowHuman;
    public Node currentNode;
    public ConditionNode nowCondition;

    //public Coroutine chatting;
    public bool isChattingRunning = false;

    private void Start()
    {
        for (int i = 0; i < chatContainer.chatTrees.Count; i++)
        {
            ChatTree chatTree = chatContainer.chatTrees[i];
            for (int j = 0; j < chatTree.nodeList.Count; j++)
            {
                chatTree.nodeList[j].is_UseThis = false;
                chatTree.nodeList[j].test_isRead = false;
            }
        }
    }

    public void AddHuman(string who)     // HG
    {
        GameManager.Instance.chatSystem.AddMember(who);
    }

    private void Update()
    {
        if (is_ChatStart)
        {
            currentTime += Time.deltaTime;
            if (Input.GetMouseButtonDown(0) || currentTime >= nextChatTime)
            {
                // node list
                var children = chatContainer.GetChatTree().GetChild(currentNode);

                foreach (Node node in children)
                {
                    if (node is ChatNode chatNode)
                    {
                        if (chatNode.test_isRead == false)
                        {
                            // Input chat
                            GameManager.Instance.chatSystem.InputChat(nowHumanName, chatNode.state,
                                chatNode.type, chatNode.face, chatNode.chatText, chatNode.textEvent);

                            // chat event
                            GameManager.Instance.chatSystem.SettingChat(nowHuman, chatNode, chatNode.face, chatNode.textEvent);

                            // load next
                            currentNode = children[0];
                            children[0].test_isRead = true;
                        }
                    }
                    else if (node is AskNode askNode)
                    {
                        if (askNode.test_isRead == false && askNode.is_UseThis == false)
                        {
                            // 筌왖疫?筌욌뜄揆 ?봔筌?揶쎛?紐???
                            currentNode = askNode.parent;
                            nowHuman.memCurrentNode = askNode.parent;

                            // 筌욌뜄揆 ??밴쉐
                            GameManager.Instance.chatSystem.InputQuestion(nowHumanName, true, askNode);
                            nowHuman.questions.Add(askNode);
                            askNode.test_isRead = true;

                            is_ask = true;
                        }
                    }
                    else if (node is ConditionNode conditionNode)
                    {
                        // When allquestion condition
                        if (conditionNode.is_AllQuestion)
                        {
                            if (conditionNode.asks.Count > 0)
                            {
                                // when all question is useThis true
                                if (conditionNode.Checkk())
                                {
                                    conditionNode.is_UseThis = true;
                                    currentNode = conditionNode;
                                }
                                else
                                {
                                    currentNode = conditionNode.asks[0].parent;
                                    StartChatting();
                                }
                            }
                            else
                                Debug.LogError("not exist question, but exist question condition");
                        }
                        else if (conditionNode.is_SpecificFile)
                        {
                            if (conditionNode.is_UseThis == false)
                            {
                                //Debug.Log("file trigger off");
                                nowCondition = conditionNode;
                                StopChatting();
                            }
                            else
                            {
                                //Debug.Log("file trigger on");
                                currentNode = conditionNode;
                            }
                        }
                        else if (conditionNode.is_LockQuestion)
                        {
                            AskNode ask = conditionNode.childList[0] as AskNode;

                            if (conditionNode.childList[0].test_isRead == false && conditionNode.childList[0].is_UseThis == false)
                            {
                                GameManager.Instance.chatSystem.InputQuestion(nowHumanName, false, ask);
                                nowHuman.questions.Add(ask);
                                conditionNode.childList[0].test_isRead = true;
                            }
                        }
                    }

                }

                if (is_ask)
                {
                    StopChatting();
                    is_ask = false;
                }

                currentTime = 0f;
            }
        }
        else currentTime = 0f;
    }

    public void ChatResetAndStart(string name)      // HG
    {
        Debug.Log("?紐꺪?癰궰野?");

        nowHumanName = name;
        nowHuman = GameManager.Instance.chatSystem.FindMember(nowHumanName);

        chatContainer.nowName = name;
        nowNodes = chatContainer.GetChatTree().nodeList;

        if (nowNodes[0] is RootNode rootNode)
        {
            if (nowHuman.memCurrentNode != null)          // return human
            {
                currentNode = nowHuman.memCurrentNode;
            }
            else
                currentNode = rootNode;
        }

        StartChatting();
    }

    public void StartChatting()
    {
        is_ChatStart = true;
    }

    public void StopChatting()
    {
        is_ChatStart = false;
    }
}
