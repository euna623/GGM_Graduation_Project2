using ChatVisual;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public enum FileType
{
    FOLDER,
    IMAGE,
    TEXT
}

[Serializable]
public class FolderFile
{
    public string folderName;
    public string parentFolderName;
    public List<string> folderFiles;
    public List<string> textFiles;
    public List<string> imageFiles;
    //public List<VisualElement> folderFiles;
    //public List<VisualElement> textFiles;
    //public List<VisualElement> imageFiles;

    public FolderFile(string name)
    {
        folderName = name;
        parentFolderName = "Main";
        folderFiles = new List<string>();
        textFiles = new List<string>();
        imageFiles = new List<string>();
    }
}

public class UIReader_FileSystem : UI_Reader
{
    [SerializeField]
    private float fileAreaSizeOn, fileAreaSizeOff;
    [SerializeField]
    private Texture2D changeSizeBtnOn, changeSizeBtnOff;
    public bool isFileSystemOpen;

    Tween changeFileSystemSizeDOT;



    // UXML
    VisualElement ui_fileSystemArea;
    VisualElement ui_fileGround;
    VisualElement ui_filePathGround;
    VisualElement ui_mainFilePath;
    VisualElement ui_panelGround;
    [HideInInspector] public Button ui_changeSizeButton;



    // Template
    [Header("Template")]
    [SerializeField] VisualTreeAsset ux_filePath;
    [SerializeField] VisualTreeAsset ux_fileGround;
    [SerializeField] VisualTreeAsset ux_folderFile;
    [SerializeField] VisualTreeAsset ux_imageFile;
    [SerializeField] VisualTreeAsset ux_textFile;
    [SerializeField] VisualTreeAsset ux_ImagePanel;
    [SerializeField] VisualTreeAsset ux_TextPanel;



    // folder
    private string currentFolderName;
    [SerializeField] List<FolderFile> fileFolders; // this is test, fileFolders -> fileFolderList X!!
    // if dictionary is correct working, remove fileFolders!
    public Dictionary<string, FolderFile> fileFolderList;

    // path
    private Stack<string> filePathLisk;
    public FolderFile currentFileFolder;

    // file drag and drop
    private VisualElement fileDefaultArea;
    private List<VisualElement> lockQuestions;

    private void Awake()
    {
        base.Awake();

        fileFolders = new List<FolderFile>();
        fileFolderList = new Dictionary<string, FolderFile>();
        filePathLisk = new Stack<string>();
        lockQuestions = new List<VisualElement>();

        MinWidth = 500f;
        MinHeight = 500f;
        MaxWidth = 1800f;
        MaxHeight = 980f;
}

    private void OnEnable()
    {
        base.OnEnable();

        UXML_Load();
        Event_Load();

        fileFolders.Add(new FolderFile("Main"));
        fileFolderList.Add("Main", new FolderFile("Main"));
        AddFilePath("Main");
    }

    private void UXML_Load()
    {
        ui_fileSystemArea = root.Q<VisualElement>("FileSystem");
        ui_fileGround = root.Q<VisualElement>("FileGround");
        ui_filePathGround = root.Q<VisualElement>("FilePathGround");
        ui_panelGround = root.Q<VisualElement>("PanelGround");
        ui_changeSizeButton = root.Q<Button>("ChangeSize");
    }

    private void Event_Load()
    {
        //AddFolderGround("Main");
        //AddFilePath("Main", () => FolderPathEvent("Main"));

        ui_changeSizeButton.clicked += () =>
        {
            OnOffFileSystem(0.25f);
        };
    }

    private void FindLockQuestion()
    {
        MemberProfile member = GameManager.Instance.chatSystem.FindMember(GameManager.Instance.chatSystem.currentMemberName);
        Debug.Log(member.name);
        for (int i = 0; i < member.quetions.Count; ++i)
        {
            if (member.quetions[i].chatType == EChatType.LockQuestion)
                lockQuestions.Add(GameManager.Instance.chatSystem.ui_questionGround.ElementAt(i));
        }
    }

/*    private LockAskAndReply FindQuestion(VisualElement ask)
    {
        Chapter chapter = chapterManager.FindChapter(chatSystem.FindMember(chatSystem.currentMemberName).chapterName);
        List<LockAskAndReply> asks = chapter.lockAskAndReply;
        for (int i = 0; i < chapter.lockAskAndReply.Count; ++i)
        {
            if (chapter.lockAskAndReply[i].ask == ask.name)
                return chapter.lockAskAndReply[i];
        }

        return new LockAskAndReply();
    }*/

    private VisualElement FindMoveArea(Vector2 position)
    {
        // ??遺덈젮吏?붾벏 
        FindLockQuestion();
        //Debug.Log(lockQuestions.Count);

        //紐⑤뱺 ?щ’??李얠븘??洹몄쨷?먯꽌 worldBound ??position???랁븯????앹쓣 李얠븘?ㅻ㈃
        foreach (VisualElement moveArea in lockQuestions)
        {
            if (moveArea.worldBound.Contains(position)) //?대떦 RECT?덉뿉 ?ъ??섏씠 ?덈뒗吏 寃?ы빐
            {
                Debug.Log(moveArea.Q<Label>().text);
                return moveArea;
            }
        }
        return null;
    }

    private void LoadDragAndDrop(VisualElement file, Action action)
    {
        // ?쒕옒洹????쒕∼ 湲곕뒫 異붽?
        file.AddManipulator(new Dragger((evt, target, beforeSlot) =>
        {
            var area = FindMoveArea(evt.mousePosition);
            Debug.Log(area.name + " area ?대쫫");
            target.RemoveFromHierarchy();
            if (area == null)
            {
                beforeSlot.Add(target);
            }
            else
                Debug.Log("?좉릿 吏덈Ц怨?遺?ろ옒");
        },
        () => { action(); }
        ));
    }

    private FolderFile FindFolder(string name)
    {
        //foreach (FolderFile folder in fileFolders)
        //{
        //    if (folder.folderName == name)
        //        return folder;
        //}

        return fileFolderList[name];
    }

    public void AddFile(FileType fileType, string fileName, string fileParentName)
    {
        // find parentFolder
        FolderFile parentFolder = FindFolder(fileParentName);

        // if exist parenteFolder
        if (parentFolder != null)
        {
            // register folder to parentFolder
            switch (fileType)
            {
                case FileType.FOLDER:
                        fileFolders.Add(new FolderFile(fileName));
                    fileFolderList.Add(fileName, new FolderFile(fileName));
                    parentFolder.folderFiles.Add(fileName);
                    break;
                case FileType.IMAGE:
                    parentFolder.imageFiles.Add(fileName);
                    break;
                case FileType.TEXT:
                    parentFolder.textFiles.Add(fileName);
                    break;
            }
        }
        // if not exist parenteFolder
        else
        {
            FolderFile newParentFolder = CreateNewParent(fileType, fileParentName, fileName);
            if (newParentFolder != null)
                AddFile(fileType, fileName, newParentFolder.folderName);
            else
                Debug.Log("FindParent error, unable to make parent");

            //switch (fileType)
            //{
            //    case FileType.FOLDER:
            //        newParentFolder.folderFiles.Add(file);
            //        break;
            //    case FileType.IMAGE:
            //        newParentFolder.imageFiles.Add(file);
            //        break;
            //    case FileType.TEXT:
            //        newParentFolder.textFiles.Add(file);
            //        break;
            //}
        }

        ////VisualElement file = null;

        //// file type
        //switch (fileType)
        //{
        //    // if folder
        //    case FileType.FOLDER:
        //        {
        //            //// create uxml
        //            //file = RemoveContainer(ux_folderFile.Instantiate());
        //            //// change file name
        //            //file.Q<Label>("FileName").text = fileName;
        //            //// connection click event
        //            //file.Q<Button>("FileImage").clicked += () => 
        //            //{ 
        //            //    // draw current foluder
        //            //    DrawFile(file.Q<Label>("FileName").text); 
        //            //    // add current folder path
        //            //    AddFilePath(fileName); 
        //            //};

        //            AddParenet(fileType, fileParentName, fileName, file);
        //            break;
        //        }
        //        // 후.. 너희는 이런 거 하지마라...
        //    case FileType.IMAGE:
        //        {
        //            //// create uxml
        //            //file = RemoveContainer(ux_imageFile.Instantiate());
        //            //// change file name
        //            //file.Q<Label>("FileName").text = fileName;
        //            //// connection drag and drop & button click event
        //            //file.AddManipulator(new Dragger((evt, target, beforeSlot) =>
        //            //{
        //            //    var area = FindMoveArea(evt.mousePosition);
        //            //    target.RemoveFromHierarchy();
        //            //    if (area == null)
        //            //    {
        //            //        beforeSlot.Add(target);
        //            //    }
        //            //    else
        //            //    {
        //            //    /*LockAskAndReply lockQuestion = FindQuestion(area);
        //            //        if (FindFile(fileName).lockQuestionName == lockQuestion.ask)
        //            //        {
        //            //            area.parent.Remove(area);
        //            //            chatSystem.InputQuestion((chatSystem.FindMember(chatSystem.currentMemberName).nickName),
        //            //                EChatType.Question, lockQuestion.ask, true, chapterManager.InputCChat(true, lockQuestion.reply));
        //            //        }
        //            //        else
        //            //            beforeSlot.Add(target);*/
        //            //    }
        //            //},
        //            //() => { ImageEvent(file); }
        //            //));

        //            AddParenet(fileType, fileParentName, fileName, file);

        //            //// ?뚯씪 遺紐?吏??
        //            //bool addNew = false;
        //            ////foreach (FolderFile folder in fileFolders)
        //            ////{
        //            ////    // if you discover parent folder
        //            ////    if (folder.folderName != fileParentName)
        //            //FolderFile folder = fileFolderList[fileParentName];
        //            //if (folder != null)
        //            //{
        //            //    folder.imageFiles.Add(file);
        //            //    addNew = true;
        //            //    break;
        //            //}
        //            ////}

        //            //// ?대뜑 ?앹꽦 諛?異붽?
        //            //// if parentfolder not exist? new add
        //            //if (addNew == false)
        //            //{
        //            //    AddFile(FileType.FOLDER, fileParentName, "Main");
        //            //    AddFile(fileType, fileName, fileParentName);
        //            //}
        //            break;
        //        }
        //    case FileType.TEXT:
        //        {
        //          //  // ?앹꽦
        //          //  file = RemoveContainer(ux_textFile.Instantiate());
        //          //  // ?대쫫 蹂寃?
        //          //  file.Q<Label>("FileName").text = fileName;
        //          //  // ?쒕옒洹????쒕∼ 湲곕뒫 異붽?
        //          //  file.AddManipulator(new Dragger((evt, target, beforeSlot) =>
        //          //  {
        //          //      var area = FindMoveArea(evt.mousePosition);
        //          //      target.RemoveFromHierarchy();
        //          //      if (area == null)
        //          //      {
        //          //          beforeSlot.Add(target);
        //          //      }
        //          //      else
        //          //      {
        //          ///*          LockAskAndReply lockQuestion = FindQuestion(area);
        //          //          if (FindFile(fileName).lockQuestionName == lockQuestion.ask)
        //          //          {
        //          //              area.parent.Remove(area);
        //          //              chatSystem.InputQuestion((chatSystem.FindMember(chatSystem.currentMemberName).nickName),
        //          //                  EChatType.Question, lockQuestion.ask, true, chapterManager.InputCChat(true, lockQuestion.reply));
        //          //          }*/
        //          //          //else
        //          //            //  beforeSlot.Add(target);
        //          //      }
        //          //  },
        //          //  () => { TextEvent(file); }
        //          //  ));

        //            AddParenet(fileType, fileParentName, fileName, file);

        //            //// ?뚯씪 遺紐?吏??
        //            //bool addNew = false;
        //            //FolderFile folder = fileFolderList[fileParentName];
        //            //if (folder != null)
        //            //{
        //            //    folder.imageFiles.Add(file);
        //            //    addNew = true;
        //            //    break;
        //            //}

        //            //// ?대뜑 ?앹꽦 諛?異붽?
        //            //if (addNew == false)
        //            //{
        //            //    //FolderFile folderParent = new FolderFile(fileParentName);
        //            //    //fileFolders.Add(folderParent);
        //            //    //fileFolders.Add(new FolderFile(fileName));
        //            //    //folderParent.folderFiles.Add(file);

        //            //    AddFile(FileType.FOLDER, fileParentName, "Main");
        //            //    AddFile(fileType, fileName, fileParentName);
        //            //}
        //            break;
        //        }
        //}

        if (currentFolderName == "")
            currentFolderName = fileParentName;
        if (currentFolderName == fileParentName)
            DrawFile(currentFolderName);
    }

    private FolderFile CreateNewParent(FileType fileType, string fileParentName, string fileName)
    {
            // file - parent
        // create new parentFolder
        FolderFile newParentFolder = new FolderFile(fileParentName);

        // register parentFolder
            fileFolders.Add(newParentFolder);
        fileFolderList.Add(newParentFolder.folderName, newParentFolder);
        
        // add file to newParentFolder
        switch (fileType)
        {
            case FileType.FOLDER:
                newParentFolder.folderFiles.Add(fileName);
                break;
            case FileType.IMAGE:
                newParentFolder.imageFiles.Add(fileName);
                break;
            case FileType.TEXT:
                newParentFolder.textFiles.Add(fileName);
                break;
        }



            // parent - super parent
        // find super parent name
        string superParentFolderName = GameManager.Instance.fileManager.FindFile(fileParentName).fileParentName;
        // find super parent
        FolderFile superParentFolder = FindFolder(superParentFolderName);
        // if superParent is exist, add newParentFolder to it's parent
        if (superParentFolder != null)
            superParentFolder.folderFiles.Add(newParentFolder.folderName);
        // if superParent isn't exist, add newParentFolder to it's new parent
        else
            CreateNewParent(FileType.FOLDER, superParentFolderName, fileParentName)
                .folderFiles.Add(newParentFolder.folderName);



        // return new parent
        return newParentFolder;
    }

    public void DrawFile(string folderName)
    {
            // fileGround - current folder ground
            // fileFolders - current folder list
            // folderName - current folder name

        // change current folder
        currentFolderName = folderName;

        // all file remove of fileGround
        RemoveFile();

        // find current folder
        currentFileFolder = fileFolderList[folderName];

        // folder isn't null
        if (currentFileFolder != null)
        {
            VisualElement file = null;

            // create current folder's childen
            foreach (string folderName1 in currentFileFolder.folderFiles)
            {
                File folder = GameManager.Instance.fileManager.FindFile(folderName1);
                // create uxml
                file = RemoveContainer(ux_folderFile.Instantiate());
                // change file name
                file.Q<Label>("FileName").text = folder.fileName;
                // connection click event
                file.Q<Button>("FileImage").clicked += () =>
                {
                    // draw current foluder
                    DrawFile(file.Q<Label>("FileName").text);
                    // add current folder path
                    AddFilePath(folder.fileName);
                };

                // add file
                ui_fileGround.Add(file);
            }

            foreach (string imageName in currentFileFolder.imageFiles)
            {
                File image = GameManager.Instance.fileManager.FindFile(folderName);
                // create uxml
                file = RemoveContainer(ux_imageFile.Instantiate());
                // change file name
                file.Q<Label>("FileName").text = image.fileName;
                // connection drag and drop & button click event
                file.AddManipulator(new Dragger((evt, target, beforeSlot) =>
                {
                    var area = FindMoveArea(evt.mousePosition);
                    target.RemoveFromHierarchy();
                    if (area == null)
                        beforeSlot.Add(target);
                    else
                    {
                        /*LockAskAndReply lockQuestion = FindQuestion(area);
                            if (FindFile(fileName).lockQuestionName == lockQuestion.ask)
                            {
                                area.parent.Remove(area);
                                chatSystem.InputQuestion((chatSystem.FindMember(chatSystem.currentMemberName).nickName),
                                    EChatType.Question, lockQuestion.ask, true, chapterManager.InputCChat(true, lockQuestion.reply));
                            }
                            else
                                beforeSlot.Add(target);*/
                    }
                },
                () => { ImageEvent(file); }
                ));

                // add file
                ui_fileGround.Add(file);
            }

            foreach (string textName in currentFileFolder.textFiles)
            {
                File text = GameManager.Instance.fileManager.FindFile(folderName);
                // create uxml
                file = RemoveContainer(ux_textFile.Instantiate());
                // change file name
                file.Q<Label>("FileName").text = text.fileName;
                // connection drag and drop & button click event
                file.AddManipulator(new Dragger((evt, target, beforeSlot) =>
                {
                    var area = FindMoveArea(evt.mousePosition);
                    target.RemoveFromHierarchy();
                    if (area == null)
                        beforeSlot.Add(target);
                },
                () => { TextEvent(file); }
                ));

                // add file
                ui_fileGround.Add(file);
            }
        }
    }

    private void RemoveFile()
    {
        for (int i = ui_fileGround.childCount - 1; i >= 0; i--)
            ui_fileGround.RemoveAt(i);
    }

    public void OpenImage(string name, Sprite sprite)
    {
        for (int i = ui_panelGround.childCount - 1; i >= 0; i--)
            ui_panelGround.RemoveAt(i);

        VisualElement panel = RemoveContainer(ux_ImagePanel.Instantiate());
        panel.Q<Label>("Name").text = name + ".png";  
        panel.Q<VisualElement>("Image").style.backgroundImage = new StyleBackground(sprite);
        ReSizeImage(panel.Q<VisualElement>("Image"), sprite);
        panel.Q<Button>("CloseBtn").clicked += () => 
        { 
            ui_panelGround.Remove(panel);
        
            File file = GameManager.Instance.fileManager.FindFile(name);
            GameManager.Instance.fileManager.UnlockChat(file);
            GameManager.Instance.fileManager.UnlockChapter(file);

            StartCoroutine(GameManager.Instance.chapterManager.ReadChat());
        };


        ui_panelGround.Add(panel);
        StopCoroutine(GameManager.Instance.chapterManager.chatting);
    }

    public void OpenText(string name, string text)
    {
        VisualElement panel = RemoveContainer(ux_TextPanel.Instantiate());
        panel.Q<Label>("Name").text = name + ".text";
        panel.Q<Label>("Text").text = text;
        panel.Q<Button>("CloseBtn").clicked += () => 
        { 
            ui_panelGround.Remove(panel);

            File file = GameManager.Instance.fileManager.FindFile(name);
            GameManager.Instance.fileManager.UnlockChat(file);
            GameManager.Instance.fileManager.UnlockChapter(file);
        };

        ui_panelGround.Add(panel);
    }

    private void AddFilePath(string pathName)
    {
        VisualElement filePath = RemoveContainer(ux_filePath.Instantiate());
        filePath.Q<Button>().text = pathName + "> ";
        filePath.Q<Button>().clicked += () => { FolderPathEvent(pathName); };
        ui_filePathGround.Add(filePath);
        filePathLisk.Push(pathName);
    }

    private void FolderPathEvent(string fileName)
    {
        while (true)
        {
            if (filePathLisk.Peek() == fileName)
                break;
            ui_filePathGround.RemoveAt(ui_filePathGround.childCount - 1);
            filePathLisk.Pop();
        }

        DrawFile(filePathLisk.Peek());
    }

    public void ImageEvent(VisualElement file)
    {
        //OpenPanel(imageFindingPanel);
        GameManager.Instance.imageSystem.EventImage(file);
    }

    public void TextEvent(VisualElement file)
    {
        string name = file.Q<Label>("FileName").text;
        //OpenText(name, GameManager.Instance.imageManager.memoDic[name]);
    }

    public void OnOffFileSystem(float during)
    {
        isFileSystemOpen = !isFileSystemOpen;

        if (changeFileSystemSizeDOT != null)
        {
            changeFileSystemSizeDOT.Complete();
            changeFileSystemSizeDOT = null;
        }

        if (isFileSystemOpen)
        {
            changeFileSystemSizeDOT = DOTween.To(() => ui_fileSystemArea.style.flexBasis.value.value, x => 
                ui_fileSystemArea.style.flexBasis = x, fileAreaSizeOn, during);
            ui_changeSizeButton.style.backgroundImage = new StyleBackground(changeSizeBtnOn);
        }
        else
        {
            changeFileSystemSizeDOT = DOTween.To(() => ui_fileSystemArea.style.flexBasis.value.value, x => 
                ui_fileSystemArea.style.flexBasis = x, fileAreaSizeOff, during);
            ui_changeSizeButton.style.backgroundImage = new StyleBackground(changeSizeBtnOff);
        }
    }
}