#r "System.Text.Encoding"
#r "System.Runtime"
#r "System.Threading.Tasks"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mustache;

public class ParsedFile
{
    private SyntaxNode rootNode;
    private CSharpCompilation compilation;
    private SemanticModel model;

    public ParsedFile(string file, string directory)
    {
        Tree = CSharpSyntaxTree.ParseText(file);
        Directory = directory;
    }

    public string Directory { get; }

    public SyntaxTree Tree { get; }

    public SyntaxNode RootNode
    {
        get
        {
            if (rootNode == null)
                rootNode = Tree.GetRoot();
            return rootNode;
        }
    }

    public CSharpCompilation Compilation
    {
        get
        {
            if (compilation == null)
            {
                compilation = CSharpCompilation
                    .Create("temp", new[] { Tree })
                    .AddReferences(new MetadataReference[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
            }
            return compilation;
        }
    }

    public SemanticModel Model
    {
        get
        {
            if (model == null)
                model = Compilation.GetSemanticModel(Tree, false);
            return model;
        }
    }

    public List<ClassDeclarationSyntax> Classes
    {
        get { return RootNode.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList(); }
    }

    public List<EnumDeclarationSyntax> Enums
    {
        get { return RootNode.DescendantNodes().OfType<EnumDeclarationSyntax>().ToList(); }
    }

    public List<InterfaceDeclarationSyntax> Interfaces
    {
        get { return RootNode.DescendantNodes().OfType<InterfaceDeclarationSyntax>().ToList(); }
    }
}

public class Entity
{
    public Entity()
    {
        Properties = new List<EntityProperty>();
    }
    public string Name { get; set; }
    public EntityType Type { get; set; }
    public List<EntityProperty> Properties { get; set; }
    public class EntityProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object ConstantValue { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimitive { get; set; }
    }

    public enum EntityType
    {
        @class,
        @enum
    }
}

public class TabTagDefinition : InlineTagDefinition
{
    public TabTagDefinition()
        : base("tab")
    {
    }

    public override void GetText(TextWriter writer, Dictionary<string, object> arguments, Scope context)
    {
        writer.Write("\t");
    }
}

static List<ParsedFile> ParsedFiles { get; set; }

static string classTemplate = 
    @"{{#each classes}}
    export interface {{name}} {{{#newline}}
    {{#each classMembers}}
        {{#tab}}{{name}}{{nullable}}: {{type}};{{#newline}}
    {{/each}}
    }{{#newline}}
    {{/each}}";

static string enumTemplate = 
    @"{{#each enums}}
    export enum {{name}} {{{#newline}}
    {{#each enumMembers}}
        {{#tab}}{{name}} = {{value}},{{#newline}}
    {{/each}}
    }{{#newline}}
    {{/each}}";

static void ParseDirectory(string dir, string outputFile, string classTemplatePath, string enumTemplatePath)
{
    if(string.IsNullOrEmpty(dir))
    {
        throw new ArgumentException("Folder to scan is required");
    }
    else if(!Directory.Exists(dir))
    {
        throw new ArgumentException("Folder to scan does not exist");
    }

    if(string.IsNullOrEmpty(outputFile))
    {
        outputFile = Path.Combine(dir, "dto.d.ts");
    }

    if(!string.IsNullOrEmpty(classTemplatePath) && !File.Exists(classTemplatePath))
    {
        throw new ArgumentException("ClassTemplate file does not exist");
    }

    if (!string.IsNullOrEmpty(enumTemplatePath) && !File.Exists(enumTemplatePath))
    {
        throw new ArgumentException("EnumTemplate file does not exist");
    }

    ParsedFiles = new List<ParsedFile>();
    ParseFiles(dir);
    foreach(var subdir in Directory.GetDirectories(dir))
    {
        ParseFiles(subdir);
    }

    List<Entity> entities = new List<Entity>();
    foreach (var files in ParsedFiles.GroupBy(o => o.Directory))
    {
        foreach (var file in files)
        {
            entities.AddRange(ProcessFile(file));
        }
    }

    RenderDto(entities, outputFile, classTemplatePath, enumTemplatePath);
}

static void ParseFiles(string dir)
{
    var files = Directory.GetFiles(dir, "*.cs");
    foreach (var filePath in files)
    {
        var file = File.ReadAllText(filePath);
        ParsedFiles.Add(new ParsedFile(file, dir));
    }
}

static List<Entity> ProcessFiles()
{
    List<Entity> entities = new List<Entity>();
    foreach (var files in ParsedFiles.GroupBy(o => o.Directory))
    {
        foreach (var file in files)
        {
            entities.AddRange(ProcessFile(file));
        }
    }
    return entities;
}

static List<Entity> ProcessFile(ParsedFile file)
{
    List<Entity> entities = new List<Entity>();
    foreach (var cls in file.Classes)
    {
        entities.Add(ProcessClass(cls, file.Model));
    }

    foreach(var e in file.Enums)
    {
        entities.Add(ProcessEnum(e, file.Model));
    }
    return entities;
}

static Entity ProcessClass(ClassDeclarationSyntax cls, SemanticModel model)
{
    var classSymbol = model.GetDeclaredSymbol(cls);
    var entity = new Entity { Type = Entity.EntityType.@class };
    entity.Name = classSymbol.Name;

    var properties = cls.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
    foreach (var prop in properties)
    {
        var propertySymbol = model.GetDeclaredSymbol(prop);
        entity.Properties.Add(new Entity.EntityProperty
        {
            Name = propertySymbol.Name,
            IsNullable = IsNullable(prop.Type),
            Type = ConvertToTypescriptType(prop.Type, model)
        });
    }
    return entity;
}

static Entity ProcessEnum(EnumDeclarationSyntax e, SemanticModel model)
{
    var enumSymbol = model.GetDeclaredSymbol(e);
    var entity = new Entity { Name = enumSymbol.Name, Type = Entity.EntityType.@enum };
    var enumMembers = e.DescendantNodes().OfType<EnumMemberDeclarationSyntax>().ToList();
    foreach (var enumMember in enumMembers)
    {
        var memberSymbol = model.GetDeclaredSymbol(enumMember);
        entity.Properties.Add(new Entity.EntityProperty { Name = memberSymbol.Name, ConstantValue = memberSymbol.ConstantValue });
    }
    return entity;
}

static void RenderDto(List<Entity> entities, string outputFile, string classTemplatePath, string enumTemplatePath)
{
    var dtoBuilder = new StringBuilder();
    var classes = new List<object>();
    foreach(var entity in entities.Where(o => o.Type == Entity.EntityType.@class))
    {
        var classMembers = new List<object>();
        foreach(var member in entity.Properties)
        {
            classMembers.Add(new
            {
                name = member.Name,
                nullable = member.IsNullable ? "?" : "",
                type = member.Type
            });
        }

        classes.Add(new
        {
            name = entity.Name,
            classMembers = classMembers.ToArray()
        });
    }
    dtoBuilder.Append(CompileTemplate(GetTemplate(classTemplatePath, classTemplate), new { classes = classes.ToArray() }));

    var enums = new List<object>();
    foreach(var entity in entities.Where(o => o.Type == Entity.EntityType.@enum))
    {
        var enumMembers = new List<object>();
        foreach (var member in entity.Properties)
        {
            enumMembers.Add(new
            {
                name = member.Name,
                value = member.ConstantValue
            });
        }

        enums.Add(new
        {
            name = entity.Name,
            enumMembers = enumMembers.ToArray()
        });
    }
    dtoBuilder.Append(CompileTemplate(GetTemplate(enumTemplatePath, enumTemplate), new { enums = enums.ToArray() }));

    if (Path.IsPathRooted(outputFile) && !Directory.Exists(Path.GetDirectoryName(outputFile)))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
    }

    File.WriteAllText(outputFile, dtoBuilder.ToString());
}

static string GetTemplate(string templatePath, string template)
{
    return !string.IsNullOrEmpty(templatePath) ? File.ReadAllText(templatePath) : enumTemplate;
}

static string CompileTemplate(string template, object data)
{
    Regex r = new Regex(@"^\s+", RegexOptions.Multiline);
    template = r.Replace(template, string.Empty);

    var compiler = new FormatCompiler();
    compiler.RegisterTag(new TabTagDefinition(), true);
    var generator = compiler.Compile(template);
    return generator.Render(data);
}

static string ConvertToTypescriptType(TypeSyntax type, SemanticModel model)
{
    var typeInfo = model.GetTypeInfo(type);
    var tsType = "";

    if (type as ArrayTypeSyntax != null)
    {
        var ats = type as ArrayTypeSyntax;
        tsType = GetTsType(model.GetTypeInfo(ats.ElementType).Type);

        if (!string.IsNullOrEmpty(tsType))
        {
            tsType = $"Array<{tsType}>";
        }
    }
    else if(type as NullableTypeSyntax != null)
    {
        var nts = type as NullableTypeSyntax;
        tsType = GetTsType(model.GetTypeInfo(nts.ElementType).Type);
    }
    else
    {
        tsType = IsBasicTsType(typeInfo.Type);
        if(string.IsNullOrEmpty(tsType))
        {
            if (IsKnownType(typeInfo.Type.Name))
            {
                tsType = typeInfo.Type.Name;
            }
            else if (typeInfo.Type.ContainingNamespace.ToDisplayString().StartsWith("System.Collections"))
            {
                var listType = "";
                if (typeInfo.Type.ContainingNamespace.Name.StartsWith("System.Collections.Generic.Dictionary"))
                { }
                else
                {
                    var typeArguments = type.DescendantNodes().OfType<TypeArgumentListSyntax>().ToList();
                    foreach (var ta in typeArguments)
                    {
                        listType = GetTsType(model.GetTypeInfo(ta.Arguments.First()).Type);
                    }
                }

                if(!string.IsNullOrEmpty(listType))
                {
                    tsType = $"Array<{listType}>";
                }
            }
        }
    }

    if(string.IsNullOrEmpty(tsType))
    {
        tsType = "any";
    }

    return tsType;
}

static string GetTsType(ITypeSymbol typeSymbol)
{
    var tsType = IsBasicTsType(typeSymbol);
    if (string.IsNullOrEmpty(tsType))
    {
        if (IsKnownType(typeSymbol.Name))
        {
            tsType = typeSymbol.Name;
        }
    }
    return tsType;
}

static string IsBasicTsType(ITypeSymbol t)
{
    var tsType = "";
    if (t.SpecialType == SpecialType.System_String)
    {
        tsType = "string";
    }
    else if (t.SpecialType == SpecialType.System_DateTime || t.Name == "DateTimeOffset")
    {
        tsType = "Date";
    }
    else if (t.SpecialType == SpecialType.System_Boolean)
    {
        tsType = "boolean";
    }
    else if (IsNumeric(t.SpecialType))
    {
        tsType = "number";
    }
    return tsType;
}

static bool IsNumeric(SpecialType t)
{
    HashSet<SpecialType> NumericTypes = new HashSet<SpecialType>
    {
        SpecialType.System_Int16,
        SpecialType.System_Int32,
        SpecialType.System_Int64,
        SpecialType.System_UInt16,
        SpecialType.System_UInt32,
        SpecialType.System_UInt64,
        SpecialType.System_Decimal,
        SpecialType.System_Double,
        SpecialType.System_Single,
        SpecialType.System_Byte,
        SpecialType.System_SByte
    };

    return NumericTypes.Contains(t);
}

static bool IsKnownType(string typeName)
{
    return ParsedFiles.Any(o => o.Classes.Any(c => o.Model.GetDeclaredSymbol(c).Name == typeName)
        || o.Enums.Any(e => o.Model.GetDeclaredSymbol(e).Name == typeName));
}

static bool IsNullable(TypeSyntax type)
{
    return type as NullableTypeSyntax != null;
}


ParseDirectory(
    Env.ScriptArgs.ElementAtOrDefault(0) != null ? Env.ScriptArgs[0] : "", 
    Env.ScriptArgs.ElementAtOrDefault(1) != null ? Env.ScriptArgs[1] : "",
    Env.ScriptArgs.ElementAtOrDefault(2) != null ? Env.ScriptArgs[2] : "",
    Env.ScriptArgs.ElementAtOrDefault(3) != null ? Env.ScriptArgs[3] : "");