using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

public static class SyntaxHelper
{
    /// <summary>
    /// 在一个方法定义词法序列中，向最前面的方法前追加 #region, 在最后面的方法尾部追加 #endregion
    /// </summary>
    public static IEnumerable<MethodDeclarationSyntax> WithRegionDirectiveTrivia(
        this IEnumerable<MethodDeclarationSyntax> methods, string message)
    {

        if (methods.Count() == 0)
            return methods;

        List<MethodDeclarationSyntax> list = new List<MethodDeclarationSyntax>(methods);

        var leadingTrivia = SF.TriviaList(
                SF.Trivia(
                    SF.RegionDirectiveTrivia(true)
                    .WithTrailingTrivia(new[] {
                        SF.Space,
                        SF.PreprocessingMessage(message)
            }))).AddRange(list[0].GetLeadingTrivia()); // 在旧的 LeadingTriva 之前追加 #region


        list[0] = list[0].WithLeadingTrivia(leadingTrivia);

        var lastItem = list[list.Count - 1];

        var trailingTrivia = lastItem.GetTrailingTrivia().Add(
                SF.Trivia(SF.EndRegionDirectiveTrivia(true))
            ); // 旧的 TrailingTrivia 之后追加 #endregion

        list[list.Count - 1] = list[list.Count - 1].WithTrailingTrivia(trailingTrivia);

        return list;
    }

    public static CompilationUnitSyntax AddUsingIfNotExist(this CompilationUnitSyntax namespaceDeclaration,
        string usingName)
    {
        if (namespaceDeclaration.Usings.Any(usingDirective => usingDirective.Name.GetText().ToString() == usingName))
        {
            return namespaceDeclaration;
        }

        return namespaceDeclaration.AddUsings(SF.UsingDirective(SF.ParseName(usingName)));
    }
    public static CompilationUnitSyntax AddUsingWithName(this CompilationUnitSyntax namespaceDeclaration,
        params string[] usingNameList)
    {
        return namespaceDeclaration.AddUsings(usingNameList.Select((usingName) => SF.UsingDirective(SF.ParseName(usingName))).ToArray());
    }


    public static ClassDeclarationSyntax AddAttributeIfNotExist(this ClassDeclarationSyntax classDeclaration,
        string attrIdentifierName, string attrTextArgumentList)
    {
        foreach (AttributeListSyntax attributeList in classDeclaration.AttributeLists)
        {
            if (attributeList.Attributes.Any(attribute =>
                attribute.Name.GetText().ToString() == attrIdentifierName &&
                attribute.ArgumentList.GetText().ToString() == attrTextArgumentList))
            {
                return classDeclaration;
            }
        }

        var newAttributeList = classDeclaration.AttributeLists.Add(
            SF.AttributeList(SF.SingletonSeparatedList<AttributeSyntax>(
                    SF.Attribute(SF.IdentifierName(attrIdentifierName),
                    SF.ParseAttributeArgumentList(attrTextArgumentList))
                )));

        classDeclaration = classDeclaration.WithAttributeLists(newAttributeList);

        return classDeclaration;
    }

    /// <summary>
    /// 排除重名的方法，添加进列表
    /// </summary>
    /// <param name="classDeclaration"></param>
    /// <param name="methodList"></param>
    /// <returns></returns>
    public static IEnumerable<MethodDeclarationSyntax> ConcatMethodIfNotExits(this IEnumerable<MethodDeclarationSyntax> dstList,
        IEnumerable<MethodDeclarationSyntax> methodList)
    {
        HashSet<string> names = new HashSet<string>();

        foreach (var i in dstList)
        {
            names.Add(i.Identifier.Text);
        }

        List<MethodDeclarationSyntax> ret = new List<MethodDeclarationSyntax>(dstList);

        foreach (var i in methodList)
        {
            if (names.Contains(i.Identifier.Text))
                continue;
            ret.Add(i);
        }

        return ret;
    }

    public static SeparatedSyntaxList<EnumMemberDeclarationSyntax> AddEnumMember(this SeparatedSyntaxList<EnumMemberDeclarationSyntax> list, string identifier, string exp, string summaryComment = null)
    {
        var enumItem = SF.EnumMemberDeclaration(
                default, 
                SF.ParseToken(identifier), 
                SF.EqualsValueClause(SF.ParseExpression(exp)));

        if (!string.IsNullOrEmpty(summaryComment))
        {
            enumItem = enumItem.WithLeadingTrivia(SF.ParseLeadingTrivia($"/// <summary> {summaryComment} </summary>\r\n"));
        }

        return list.Add(enumItem);
    }

    /// <summary>
    /// 尝试移除括号表达式直接取内容
    /// </summary>
    /// <param name="self"></param>
    public static ExpressionSyntax TryWithoutParenthesized(this ExpressionSyntax self)
    {
        while (self is ParenthesizedExpressionSyntax parenthesized)
            self = parenthesized.Expression;

        return self;
    }
}
