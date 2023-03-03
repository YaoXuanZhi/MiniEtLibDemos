using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

internal class OldPnlSystemWalker : CSharpSyntaxWalker
{
    string _pnlSystemClassName;
    string _filepath;

    /// <summary>
    /// 遍历旧的词法树搜集一些基本信息 
    /// </summary>
    public OldPnlSystemWalker(string pnlSystemClassName, string filepath)
    {
        _pnlSystemClassName = pnlSystemClassName;
        _filepath = filepath;
    }

    public Dictionary<string, UsingDirectiveSyntax> allUsingDirectiveSyntax = new Dictionary<string, UsingDirectiveSyntax>();

    public ClassDeclarationSyntax compolentSystemClassDeclarationSyntax;

    public NamespaceDeclarationSyntax namespaceDeclarationSyntax;

    public void Check()
    {
        if (namespaceDeclarationSyntax == null)
            return;
        if (namespaceDeclarationSyntax.Members.Count > 1)
            throw new Exception($"{_filepath} 命名空间 {namespaceDeclarationSyntax.Name} 下包含了多个定义");
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        allUsingDirectiveSyntax.Add(node.Name.ToFullString(), node);

        base.VisitUsingDirective(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.Identifier.Text == _pnlSystemClassName)
        {
            compolentSystemClassDeclarationSyntax = node;
        }

        base.VisitClassDeclaration(node);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        if (namespaceDeclarationSyntax != null)
            throw new Exception($"{_filepath} 只允许定义一个命名空间");
        namespaceDeclarationSyntax = node;
        base.VisitNamespaceDeclaration(node);
    }
}

internal class RemoveRegionDirectiveTriviaRewriter : CSharpSyntaxRewriter
{
    /// <summary>
    /// 移除掉所有的 #region 和 #endregion, 这样不会影响注释
    /// </summary>
    public RemoveRegionDirectiveTriviaRewriter() { }

    public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            return default(SyntaxTrivia);
        return base.VisitTrivia(trivia);
    }
}

internal class PnlSystemDefaultMethodRewriter : CSharpSyntaxRewriter
{
    HashSet<string> _methodNames = new HashSet<string>(); 
    string _pnlName;
    string _pnlSystemClassName;

    /// <summary>
    /// 补充缺少的默认 PnlSystem 类方法
    /// </summary>
    public PnlSystemDefaultMethodRewriter(string pnlName)
    {
        _pnlName = pnlName;
        _pnlSystemClassName = $"{_pnlName}ComponentSystem";
    }

    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.Identifier.Text != _pnlSystemClassName)
            return base.VisitClassDeclaration(node);

        foreach (var member in node.Members)
        {
            if (member is MethodDeclarationSyntax methodDeclaration)
            {
                string name = methodDeclaration.Identifier.Text;
                if (!_methodNames.Contains(name))
                    _methodNames.Add(name);
            }
        }

        if (!_methodNames.Contains("AddListener"))
        {
            node = node.AddMembers(
                SF.ParseMemberDeclaration($"public static void AddListener(this {_pnlName}Component self) {{}}"));
        }

        if (!_methodNames.Contains("RemoveListener"))
        {
            node = node.AddMembers(
                SF.ParseMemberDeclaration($"public static void RemoveListener(this {_pnlName}Component self) {{}}"));
        }

        if (!_methodNames.Contains("OnInit"))
        {
            node = node.AddMembers(
                SF.ParseMemberDeclaration($"public static void OnInit(this {_pnlName}Component self) {{}}"));
        }

        if (!_methodNames.Contains("OnDispose"))
        {
            node = node.AddMembers(
                SF.ParseMemberDeclaration($"public static void OnDispose(this {_pnlName}Component self) {{}}"));
        }
        
        if (!_methodNames.Contains("OnShow"))
        {
            node = node.AddMembers(
                SF.ParseMemberDeclaration($"public static void OnShow(this {_pnlName}Component self, params object[] args) {{}}"));
        }

        if (!_methodNames.Contains("OnHide"))
        {
            node = node.AddMembers(
                SF.ParseMemberDeclaration($"public static void OnHide(this {_pnlName}Component self) {{}}"));
        }

        return base.VisitClassDeclaration(node);
    }
}

internal class PnlSystemListenerAutoBindRewriter : CSharpSyntaxRewriter
{
    GameObject _gameObject;
    ReferenceCollector _collector;
    string _pnlName;

    /// <summary>
    /// 依赖的 Using 定义
    /// </summary>
    public List<string> dependUsing = new List<string>();
    /// <summary>
    /// UI 名称关联的方法定义
    /// </summary>
    public HashSet<string> uiNameIsMakeMethod = new HashSet<string>(); 

    public List<MethodDeclarationSyntax> allUIEventMethodDeclarations = new List<MethodDeclarationSyntax>();

    /// <summary>
    /// AddListener / RemoveListener 自动绑定代码生成
    /// </summary>
    public PnlSystemListenerAutoBindRewriter(GameObject gameObject, string pnlName) 
    {
        _gameObject = gameObject;
        _collector = _gameObject.GetComponent<ReferenceCollector>();
        _pnlName = pnlName;

        if (_collector.IsExistComponent(CollectorType.CompositeToggle))
        {
            dependUsing.Add("Mobcast.Coffee.Toggles");
        }
    }

    static readonly string ExtendCollectorLoopScrollRectClick = "LoopScrollRectClick";
    static readonly string ExtendCollectorLoopScrollRectClickEx = "LoopScrollRectClickEx";

    static string TryGetExtendComponentName(string componentName)
    {
        if (componentName == CollectorType.LoopScrollRect)
            return ExtendCollectorLoopScrollRectClick;
        if (componentName == CollectorType.LoopScrollRectMulti)
            return ExtendCollectorLoopScrollRectClickEx;
        return null;
    }

    static string GetAddListenerStatementPrefix(string componentName, string varName, string withParam = null)
    {
        if (componentName == CollectorType.Button)
            return $"self.{varName}.onClick.AddListener{(!string.IsNullOrEmpty(withParam) ? $"(self.{withParam})" : string.Empty)}";
        else if (componentName == CollectorType.UIButton)
            return $"self.{varName}.onClick.AddListener{(!string.IsNullOrEmpty(withParam) ? $"(self.{withParam})" : string.Empty)}";
        else if (componentName == CollectorType.Toggle)
            return $"self.{varName}.onValueChanged.AddListener{(!string.IsNullOrEmpty(withParam) ? $"(self.{withParam})" : string.Empty)}";
        else if (componentName == CollectorType.CompositeToggle)
            return $"self.{varName}.onValueChanged.AddListener{(!string.IsNullOrEmpty(withParam) ? $"(self.{withParam})" : string.Empty)}";
        else if (componentName == CollectorType.Dropdown)
            return $"self.{varName}.onValueChanged.AddListener{(!string.IsNullOrEmpty(withParam) ? $"(self.{withParam})" : string.Empty)}";
        else if (componentName == CollectorType.LoopScrollRect)
            return $"self.{varName}.AddUpdateEvent{(!string.IsNullOrEmpty(withParam) ? $"(self.{withParam})" : string.Empty)}";
        else if (componentName == CollectorType.LoopScrollRectMulti)
            return $"self.{varName}.AddUpdateEvent{(!string.IsNullOrEmpty(withParam) ? $"(self.{withParam})" : string.Empty)}";
        else if (componentName == ExtendCollectorLoopScrollRectClick)
            return $"self.{varName}.OnClickItem{(!string.IsNullOrEmpty(withParam) ? $" = self.{withParam}" : string.Empty)}";
        else if (componentName == ExtendCollectorLoopScrollRectClickEx)
            return $"self.{varName}.OnClickItem{(!string.IsNullOrEmpty(withParam) ? $" = self.{withParam}" : string.Empty)}";
        else
            return null;
    }

    static string GetRemoveListenerStatementPrefix(string componentName, string uiRefName, bool getParam = false)
    {
        if (componentName == CollectorType.Button)
            return $"self.{uiRefName}.onClick.RemoveAllListeners{(getParam ? "()" : String.Empty)}";
        else if (componentName == CollectorType.UIButton)
            return $"self.{uiRefName}.onClick.RemoveAllListeners{(getParam ? "()" : String.Empty)}";
        else if (componentName == CollectorType.Toggle)
            return $"self.{uiRefName}.onValueChanged.RemoveAllListeners{(getParam ? "()" : String.Empty)}";
        else if (componentName == CollectorType.CompositeToggle)
            return $"self.{uiRefName}.onValueChanged.RemoveAllListeners{(getParam ? "()" : String.Empty)}";
        else if (componentName == CollectorType.Dropdown)
            return $"self.{uiRefName}.onValueChanged.RemoveAllListeners{(getParam ? "()" : String.Empty)}";
        else if (componentName == CollectorType.LoopScrollRect)
            return $"self.{uiRefName}.AddUpdateEvent{(getParam ? "(null)" : String.Empty)}";
        else if (componentName == CollectorType.LoopScrollRectMulti)
            return $"self.{uiRefName}.AddUpdateEvent{(getParam ? "(null)" : String.Empty)}";
        else if (componentName == ExtendCollectorLoopScrollRectClick)
            return $"self.{uiRefName}.OnClickItem{(getParam ? " = null" : String.Empty)}";
        else if (componentName == ExtendCollectorLoopScrollRectClickEx)
            return $"self.{uiRefName}.OnClickItem{(getParam ? " = null" : String.Empty)}";
        else
            return null;
    }

    string GetUICallbackName(string componentName, string idName)
    {
        if (componentName == CollectorType.Button)
            return $"OnClick{idName}";
        else if (componentName == CollectorType.UIButton)
            return $"OnClick{idName}";
        else if (componentName == CollectorType.Toggle)
            return $"OnValueChange{idName}";
        else if (componentName == CollectorType.CompositeToggle)
            return $"OnValueChange{idName}";
        else if (componentName == CollectorType.Dropdown)
            return $"OnValueChange{idName}Item";
        else if (componentName == CollectorType.LoopScrollRect)
            return $"On{idName}RenderItem";
        else if (componentName == CollectorType.LoopScrollRectMulti)
            return $"On{idName}RenderItem";
        else if (componentName == ExtendCollectorLoopScrollRectClick)
            return $"OnClick{idName}Item";
        else if (componentName == ExtendCollectorLoopScrollRectClickEx)
            return $"OnClick{idName}Item";
        else
            return null;
    }

    MemberDeclarationSyntax GetUICallbackDeclaration(string componentName, string idName)
    {
        if (componentName == CollectorType.Button)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self) {{}}");
        else if (componentName == CollectorType.UIButton)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self) {{}}");
        else if (componentName == CollectorType.Toggle)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self, bool value) {{}}");
        else if (componentName == CollectorType.CompositeToggle)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self, CompositeToggle value) {{}}");
        else if (componentName == CollectorType.Dropdown)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self, int index) {{}}");
        else if (componentName == CollectorType.LoopScrollRect)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self, Transform item, int index) {{}}");
        else if (componentName == CollectorType.LoopScrollRectMulti)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self, Transform transform, TreeNode node) {{}}");
        else if (componentName == ExtendCollectorLoopScrollRectClick)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self, Transform item, int index) {{}}");
        else if (componentName == ExtendCollectorLoopScrollRectClickEx)
            return SF.ParseMemberDeclaration(
                $"public static void {GetUICallbackName(componentName, idName)}(this {_pnlName}Component self, Transform transform, TreeNode node) {{}}");
        else
            return null;
    }

    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        string methodName = node.Identifier.Text;

        if (methodName == "AddListener" || methodName == "RemoveListener")
        {
            Dictionary<string, HashSet<string>> _uiRefNames = new Dictionary<string, HashSet<string>>();

            foreach (var i in _collector.data)
            { 
                var sets = new HashSet<string>();
                sets.Add(i.component);
                string extendName;
                if (!string.IsNullOrEmpty(extendName = TryGetExtendComponentName(i.component)))
                    sets.Add(extendName);
                _uiRefNames.Add(i.key, sets);
            }

            foreach(StatementSyntax stat in node.Body.Statements)
            {
                // 移除掉已经存在的监听或者移除语句，只考虑单行绑定和移除代码
                string statStr = stat.WithoutTrivia().GetText().ToString();

                if (!statStr.StartsWith("self."))
                    continue;

                string[] arr = statStr.Split('.');

                if (_uiRefNames.TryGetValue(arr[1], out var componentSets))
                {
                    foreach (string componentName in componentSets.ToArray())
                    {
                        if (methodName == "AddListener")
                        {
                            string statPrefix = GetAddListenerStatementPrefix(componentName, arr[1]);
                            // 不支持的类型或者已声明的绑定代码
                            if (string.IsNullOrEmpty(statPrefix) || statStr.StartsWith(statPrefix))
                                componentSets.Remove(componentName);
                        }
                        else
                        {
                            string statPrefix = GetRemoveListenerStatementPrefix(componentName, arr[1]);
                            // 不支持的类型或者已声明的解绑代码
                            if (string.IsNullOrEmpty(statPrefix) || statStr.StartsWith(statPrefix))
                                componentSets.Remove(componentName);
                        }
                    }
                    
                    if (componentSets.Count == 0)
                        _uiRefNames.Remove(arr[1]);
                }
            }

            var statList = SF.List(node.Body.Statements);

            // 到这里剩下的就是缺少绑定/移除绑定语句的UI
            foreach (var i in _uiRefNames)
            {
                string idName = UICodeSpawner.UINameToIdentifierName(i.Key);

                if (!this.uiNameIsMakeMethod.Contains(i.Key))
                {
                    foreach (var componentName in i.Value)
                    {
                        // 新增回调函数定义
                        MemberDeclarationSyntax memberDeclaration = GetUICallbackDeclaration(componentName, idName);

                        if (memberDeclaration == null)
                            continue;// 不支持

                        this.allUIEventMethodDeclarations.Add(memberDeclaration as MethodDeclarationSyntax);
                        this.uiNameIsMakeMethod.Add(i.Key);
                    }
                }

                foreach (var componentName in i.Value)
                {
                    string uiCallbackName = GetUICallbackName(componentName, idName);

                    if (string.IsNullOrEmpty(uiCallbackName))
                        continue;//不支持

                    if (methodName == "AddListener")
                    {
                        statList = statList.Add(SF.ParseStatement(
                            $"{GetAddListenerStatementPrefix(componentName, i.Key, GetUICallbackName(componentName, idName))};"));
                    }
                    else
                    {
                        statList = statList.Add(SF.ParseStatement(
                            $"{GetRemoveListenerStatementPrefix(componentName, i.Key, getParam: true)};"));
                    }
                }
            }

            node = node.WithBody(node.Body.WithStatements(statList));
        }

        return base.VisitMethodDeclaration(node);
    }
}

internal static class UICodeSpawnerHelper
{
    public static bool IsUIListenerMethodName(string methodName) => methodName switch
    {
        "AddListener" or "RemoveListener" => true,
        _ => false
    };
    public static bool IsWindowEventMethodName(string methodName) => methodName switch
    {
        "OnInit" or "OnDispose" or "OnShow" or "OnHide" => true,
        _ => false
    };

    public static bool IsEventCallbackMethodName(string methodName) =>
        methodName.StartsWith("On") && !IsWindowEventMethodName(methodName);

    public static bool IsOtherMethodName(string methodName) =>
        !IsUIListenerMethodName(methodName) &&
        !IsWindowEventMethodName(methodName) &&
        !IsEventCallbackMethodName(methodName);
}

public partial class UICodeSpawner
{
    /// <summary>
    /// 更新面板对应的 PnlXXXComponentSystem 代码
    /// 自动更新根节点的 ReferenceCollector 新增的UI元素的回调绑定和解绑代码，以及自动声明空的回调函数
    /// </summary>
    static void SpawnCodeForPnlSystemEx(GameObject gameObject)
    {
        string strPnlName = panelName;
        string strFileDir = HotfixViewUIDir + strPnlName + "/";

        if (!System.IO.Directory.Exists(strFileDir))
        {
            System.IO.Directory.CreateDirectory(strFileDir);
        }

        //xxxComponentSystem
        string strFileName = string.Format("{0}ComponentSystem", strPnlName);
        string strFilePath = strFileDir + "/" + strFileName + ".cs";

        SyntaxNode syntaxTree = null;
        FileBackup codeBackup = new FileBackup(FileBackup.CODE_GENERATE);

        if (File.Exists(strFilePath))
        {
            syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(strFilePath)).GetRoot();
            string backupPath = codeBackup.Backup(strFilePath);
            Debug.Log($"已备份旧代码 {codeBackup.ToHrefText(backupPath)}");
        }
        else
        {
            // 构造空的代码
            syntaxTree = SF.CompilationUnit()
                .AddUsingIfNotExist("System")
                .AddUsingIfNotExist("System.Collections")
                .AddUsingIfNotExist("System.Collections.Generic")
                .AddUsingIfNotExist("UnityEngine")
                .AddUsingIfNotExist("UnityEngine.UI")
                .AddMembers(
                SF.ClassDeclaration(strFileName)
                    .WithModifiers(new SyntaxTokenList(
                        SF.Token(SyntaxKind.PublicKeyword),
                        SF.Token(SyntaxKind.StaticKeyword),
                        SF.Token(SyntaxKind.PartialKeyword)))
                    .AddAttributeIfNotExist("FriendOf", $"(typeof({strPnlName}Component))"));
        }

        syntaxTree = new RemoveRegionDirectiveTriviaRewriter().Visit(syntaxTree);
        syntaxTree = new PnlSystemDefaultMethodRewriter(strPnlName).Visit(syntaxTree);
        var pnlAutoBind = new PnlSystemListenerAutoBindRewriter(gameObject, strPnlName);
        syntaxTree = pnlAutoBind.Visit(syntaxTree);
        var oldPnlWalker = new OldPnlSystemWalker(strFileName, strFilePath);

        oldPnlWalker.Visit(syntaxTree);
        oldPnlWalker.Check();

        var compilationUnit = SF.CompilationUnit()
            .AddUsings(oldPnlWalker.allUsingDirectiveSyntax.Values.ToArray());

        // 添加自动绑定UI回调时可能新增的 using 依赖
        foreach (var @using in pnlAutoBind.dependUsing)
            compilationUnit = compilationUnit.AddUsingIfNotExist(@using);

        var @namespace = SF.NamespaceDeclaration(SF.IdentifierName("ET.Client"));

        // 移除旧类的所有成员，复制其定义，这样注释不会丢
        var classDeclaration = oldPnlWalker.compolentSystemClassDeclarationSyntax.WithMembers(default);

        List<MethodDeclarationSyntax> mlist = new List<MethodDeclarationSyntax>();

        // 旧的方法定义
        var oldMethods = oldPnlWalker.compolentSystemClassDeclarationSyntax.Members
            .Where(member => member is MethodDeclarationSyntax)
            .Cast<MethodDeclarationSyntax>()
            .ConcatMethodIfNotExits(pnlAutoBind.allUIEventMethodDeclarations);

        // 旧的非方法定义
        var oldNotMethods = oldPnlWalker.compolentSystemClassDeclarationSyntax.Members
            .Where(member => !(member is MethodDeclarationSyntax));

        var uiListenerMethods = oldMethods
            .Where(method => UICodeSpawnerHelper.IsUIListenerMethodName(method.Identifier.Text));

        var windowEventMethods = oldMethods
            .Where(method => UICodeSpawnerHelper.IsWindowEventMethodName(method.Identifier.Text));

        var eventCallbackMethods = oldMethods
            .Where(method => UICodeSpawnerHelper.IsEventCallbackMethodName(method.Identifier.Text));

        var otherMethods = oldMethods
            .Where(method => UICodeSpawnerHelper.IsOtherMethodName(method.Identifier.Text));

        classDeclaration = classDeclaration.AddMembers(oldNotMethods.ToArray());
        classDeclaration = classDeclaration.AddMembers(uiListenerMethods.WithRegionDirectiveTrivia("事件绑定/移除").ToArray());
        classDeclaration = classDeclaration.AddMembers(windowEventMethods.WithRegionDirectiveTrivia("界面显示/隐藏").ToArray());
        classDeclaration = classDeclaration.AddMembers(eventCallbackMethods.WithRegionDirectiveTrivia("事件回调").ToArray());
        classDeclaration = classDeclaration.AddMembers(otherMethods.WithRegionDirectiveTrivia("其它方法").ToArray());

        @namespace = @namespace.AddMembers(classDeclaration);

        compilationUnit = compilationUnit.AddMembers(@namespace);

        var code = compilationUnit.NormalizeWhitespace(indentation: "\t").ToFullString();

        File.WriteAllText(strFilePath, code);

        Debug.Log($"已生成到 {codeBackup.ToHrefText(Path.GetFullPath(strFilePath))}");
    }
}