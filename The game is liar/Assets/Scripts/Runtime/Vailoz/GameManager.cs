using System;
using System.Collections.Generic;
using UnityEngine;
using Edgar.Unity;
using UnityEngine.Tilemaps;
using System.Reflection;
using System.Collections.ObjectModel;

#if true
using StringBuilder = System.Text.StringBuilder;
#else
class StringBuilder
{
    public StringBuilder Append(object o)
    {
        return this;
    }
    public StringBuilder Append(char c)
    {
        return this;
    }
    public StringBuilder Append(string str)
    {
        return this;
    }
    
    public void AppendIndent(string str, int indent)
    {
        
    }
    
    public void AppendIndentLine(string str, int indent)
    {
        
    }
    
    public void Insert(int pos, char c)
    {
        
    }
    
    public StringBuilder(string str, int cap = 0)
    {
        
    }
    
    public StringBuilder(int cap)
    {
        
    }
    
    public int Length => 1;
    //public string ToString(int a, int b) { return null; }
}
#endif

public enum GameMode
{
    None,
    Quit,
    Main,
    Play,
    // Cutscene mode
    
    Count
}

public class GameManager : MonoBehaviour
{
    [Header("Game Mode")]
    public GameMode startMode;
    
    public GameObject mainMode;
    public GameObject playMode;
    public LevelData[] levels;
    public int currentLevel;
    
    [Header("Camera")]
    public Vector3Variable playerPos;
    public Entity cameraEntity;
    
    [Header("UI")]
    public bool overrideGameUI;
    public GameMenu gameMenu;
    
    [Header("Other")]
    public Audio[] audios;
    public AudioType firstMusic;
    public int sourceCount;
    public Pool[] pools;
    
    private List<RoomInstance> rooms;
    private int currentRoom;
    
    public static Entity player;
    public static Camera mainCam;
    public static GameUI gameUI;
    
    private static Bounds defaultBounds;
    private static Tilemap tilemap;
    
#if UNITY_EDITOR
    [EasyButtons.Button]
    private static void FindAllEntityProperties(bool generateFile)
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        System.Text.StringBuilder builder = new System.Text.StringBuilder("Date: [" + DateTime.Now.ToString() + "]\n\n", 1024 * 128);
        
        SaveEntityProperties(builder, "Prefab", () =>
                             {
                                 string[] assets = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
                                 List<Entity> entities = new List<Entity>(assets.Length);
                                 foreach (string asset in assets)
                                 {
                                     string path = UnityEditor.AssetDatabase.GUIDToAssetPath(asset);
                                     Entity entity = UnityEditor.AssetDatabase.LoadAssetAtPath<Entity>(path);
                                     if (entity)
                                         entities.Add(entity);
                                 }
                                 return entities;
                             });
        
        SaveEntityProperties(builder, "Object", () =>
                             {
                                 Entity[] assets = FindObjectsOfType<Entity>(true);
                                 List<Entity> entities = new List<Entity>(assets.Length + 1);
                                 entities.AddRange(assets);
                                 return entities;
                             });
        
        string message = "Search complete";
        UnityEngine.Object context = null;
        if (generateFile)
        {
            string path = GameUtils.CreateUniquePath("Assets/Files/properties.txt");
            System.IO.File.WriteAllText(path, builder.ToString());
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            message = "File is created at " + path;
            context = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
        watch.Stop();
        Debug.Log(message + $" after {watch.ElapsedMilliseconds}ms", context);
        static void SaveEntityProperties(System.Text.StringBuilder builder, string title, Func<List<Entity>> getEntities)
        {
            List<Entity> entities = getEntities();
            entities.Insert(0, null);
            
            builder.Append("\n");
            foreach (Entity entity in entities)
            {
                int startIndex = builder.Length;
                if (entity == null)
                    builder.Append($"-------- -------- -------- -------- {{{title}}}: {entities.Count - 1,4} -------- -------- -------- --------");
                SerializeType(entity, builder, $"-- {entity?.name} ({entity?.GetInstanceID()}) --");
                Debug.Log(builder.ToString(startIndex, builder.Length - startIndex), entity?.gameObject);
                builder.Append("\n");
            }
        }
        
        static void PrintProperty(object obj, string fieldName, System.Text.StringBuilder builder, int indentLevel)
        {
            ulong property = GameUtils.GetValueFromObject<ulong>(obj, "properties");
            string[] names = GameUtils.GetValueFromObject<string[]>(obj, "serializedEnumNames", true);
            
            builder.AppendIndentLine(fieldName, indentLevel);
            builder.AppendIndentFormat("Properties of {0}: ", indentLevel + 1, fieldName).Append(property.ToString()).Append(", ")
                .AppendLine(Convert.ToString((long)property, 2));
            
            List<string> setNames = new List<string>(names.Length);
            for (int i = 0; i < names.Length; i++)
                if (MathUtils.HasFlag(property, i))
                setNames.Add(names[i]);
            GameUtils.GetAllString(setNames, $"All set flags of {fieldName}: ", "\n", indentLevel + 1, builder: builder);
            
            GameUtils.GetAllString(names, $"Serialized names of {fieldName}: ", "\n", indentLevel + 1,
                                   toString: (name, i) => name + ": " + (MathUtils.HasFlag(property, i) ? "1" : "0"), builder: builder);
        }
        
        // NOTE: This doesn't work if the obj is already an Property
        static void SerializeType(object obj, System.Text.StringBuilder builder, string fieldName)
        {
            GameUtils.SerializeType(new object[] { obj },
                                    data => (data.type?.IsGenericType ?? false) && (data.type?.GetGenericTypeDefinition() == typeof(Property<>)), data =>
                                    {
                                        object property = data.objs[0];
                                        string name = fieldName;
                                        
                                        if (data.parent?.parent != null)
                                        {
                                            // TODO: I only handle the case when the current or parent is an element from an array. Hanlde the remaining cases.
                                            bool isArrayElement = data.parent.type.IsArray;
                                            bool isParentArrayElement = data.parent.parent.type.IsArray;
                                            // TODO: Handle array of serializable type contains array of serializable type contains array of...
                                            Debug.Assert(!(isArrayElement && isParentArrayElement));
                                            
                                            if (isParentArrayElement)
                                                data = data.parent;
                                            name = data.parent.field.Name;
                                            if (isArrayElement || isParentArrayElement)
                                                name += $"[{data.index}]";
                                        }
                                        PrintProperty(property, name, builder, data.parent.depth);
                                    }, data => data.type != null && !GameUtils.IsSerializableType(data.type), (data, recursive) => recursive());
        }
    }
    
    class NameCollisionData
    {
        static Dictionary<string, List<Type>> types;
        public static System.Collections.Concurrent.ConcurrentDictionary<int, NameCollisionData> database;
        public static List<Type> unusedAttributes;
        static List<Type> usedAttributes;
        
        Type currentType;
        public List<string> currentNamespaces;
        public List<Type> debugNamespaces;
        
        public static void Init(Assembly[] assemblies, int typeCap)
        {
            unusedAttributes = new List<Type>();
            usedAttributes = new List<Type>();
            types = new Dictionary<string, List<Type>>(typeCap);
            database = new System.Collections.Concurrent.ConcurrentDictionary<int, NameCollisionData>(16, typeCap);
            try
            {
                foreach (Assembly assembly in assemblies)
                    foreach (Type type in assembly.GetTypes())
                    if (!type.IsGenericParameter)
                    if (types.ContainsKey(type.Name))
                    types[type.Name].Add(type);
                else
                    types.Add(type.Name, new List<Type>() { type });
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.Log(e.LoaderExceptions.Length + " " + e.Types.Length);
                foreach (Exception exception in e.LoaderExceptions)
                    Debug.Log(exception);
            }
        }
        
        public static void ScopeCollision(Type currentType, Action<NameCollisionData> initNamespace, Action<NameCollisionData> action)
        {
            database.TryAdd(System.Threading.Tasks.Task.CurrentId ?? -1, new NameCollisionData()
                            {
                                currentNamespaces = new List<string>(16),
                                debugNamespaces = new List<Type>(16),
                            });
            if (data != null) initNamespace(data);
            ScopeType(currentType, () => action(data));
            data?.currentNamespaces.Clear();
            data?.debugNamespaces.Clear();
        }
        
        public static void ScopeType(Type type, Action action)
        {
            Debug.Assert(NameCollisionData.data != null, System.Threading.Tasks.Task.CurrentId);
            NameCollisionData data = NameCollisionData.data ?? new NameCollisionData();
            MathUtils.Swap(ref data.currentType, ref type);
            action();
            MathUtils.Swap(ref data.currentType, ref type);
        }
        
        private static NameCollisionData data => database?[System.Threading.Tasks.Task.CurrentId ?? -1];
        
        public static IList<Type> GetCollideTypes(string name) => types[name];
        public static Type GetCurrentType() => data.currentType;
        public static bool Contains(Type type) => types.ContainsKey(type.Name) && types[type.Name].Contains(type);
        public static bool CheckNamespace(string _namespace, bool emptyResult = false) =>
            data.currentNamespaces.Contains(_namespace) || (data.currentNamespaces.Count == 0 && emptyResult);
        
        public static void GetUnusedAttributes(MemberInfo member, IList<CustomAttributeData> data)
        {
            Attribute[] atts = Array.ConvertAll(member.GetCustomAttributes(false), att => (Attribute)att);
            for (int i = 0; i < data.Count; ++i)
                if (atts[i].GetType() != data[i].AttributeType)
                Debug.Log("Attribute: " + atts[i].GetType() + " Data: " + data[i].AttributeType);
            else
                usedAttributes.Add(data[i].AttributeType);
            
            for (int i = data.Count; i < atts.Length; ++i)
                if (!unusedAttributes.Contains(atts[i].GetType()))
                unusedAttributes.Add(atts[i].GetType());
        }
    }
    
    // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers
    // NOTE: This is in decreased restrictive level order (protected and internal have the same level)
    enum AccessModifier
    {
        None,
        Public,             // Default for interface members
        ProtectedInternal,
        Protected,
        Internal,           // Default for global types (declared directly within a namespace)
        PrivateProtected,   // (since C# 7.2)
        Private,            // Default for members (including nested types)
    }
    
    enum MemberNameMode
    {
        Default,
        NoGenericParameter,
        NoAttributePostfix,
        IsDecl,
        BaseType,
    }
    
    static System.Diagnostics.Stopwatch watch;
    
    // Because classes can't have base classes or members types (field's type, method argument and return type, etc)
    // that have lower restrictive level (public classes can't inherit from internal classes, or have a field of a internal type)
    // So Visual Studio only displays public, protected internal, and protected types/members
    // (let call these access modifiers the displayable modifier)
    // But types and members can implement any interfaces and have any attributes (including non-displayable nested ones)
    // So Visual Studio only displays definition of public/internal global types or usage of non-displayble nested types
    const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    const AccessModifier maxAccess = AccessModifier.Protected;
    
    const string unmanagedAttribute = "System.Runtime.CompilerServices.IsUnmanagedAttribute";
    const string refAttribute = "System.Runtime.CompilerServices.IsByRefLikeAttribute";
    const string readonlyAttribute = "System.Runtime.CompilerServices.IsReadOnlyAttribute";
    const string paramAttribute = "System.ParamArrayAttribute";
    const string extensionAttribute = "System.Runtime.CompilerServices.ExtensionAttribute";
    // NOTE: Check for the name because some types don't exist in .NET Standard 2.0
    static readonly string[] ignoredTypes = new string[]
    {
        paramAttribute,
        unmanagedAttribute,
        readonlyAttribute,
        refAttribute,
        extensionAttribute,
        typeof(System.Runtime.CompilerServices.DecimalConstantAttribute).FullName,
        "System.Runtime.CompilerServices.NullableAttribute",
        "System.Runtime.CompilerServices.NullableContextAttribute",
    };
    
    enum ThreadType
    {
        None,
        Assembly,
        Type,
        Load,
        All,
        Partition_Assembly,
        Partition_Type,
        Partion_Load,
        Partition_All,
    }
    
    [EasyButtons.Button]
    void GenerateHeaderFileTest(int count)
    {
        int length = Enum.GetValues(typeof(ThreadType)).Length;
        for (ThreadType threadType = ThreadType.None; (int)threadType < length; ++threadType)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            for (int i = 0; i < count; ++i)
                GenerateHeaderFile(false, false, false, threadType);
            watch.Stop();
            Debug.Log($"Type: {threadType}, Total: {watch.ElapsedMilliseconds}, Average: {(count != 0 ? watch.ElapsedMilliseconds / count : 0)}");
        }
    }
    
    static System.Threading.Tasks.Task<T> RunTask<T>(Func<T> action)
    {
        return System.Threading.Tasks.Task.Run(() =>
                                               {
                                                   UnityEngine.Profiling.Profiler.BeginThreadProfiling("Task Threads", "Thread " + System.Threading.Tasks.Task.CurrentId);
                                                   var result = action();
                                                   UnityEngine.Profiling.Profiler.EndThreadProfiling();
                                                   return result;
                                               });
    }
    
    static System.Threading.Tasks.Task RunTask(Action action)
    {
        return System.Threading.Tasks.Task.Run(action);
    }
    
    static System.Threading.Tasks.Task<T> RunTask<T>(int index, Func<int, T> action)
    {
        return RunTask(() => action(index));
    }
    
    [EasyButtons.Button]
    void GenerateHeaderFile(bool generateFile, bool writeToFolder, bool debugNamespace, ThreadType threadType)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Header File");
        watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        
        Assembly[] assemblies;
        {
            Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            bool exist = Array.Exists(_assemblies, assembly => GetName(assembly) == "DLL");
            assemblies = new Assembly[_assemblies.Length + (exist ? 0 : 1)];
            if (!exist)
            {
                byte[] content = System.IO.File.ReadAllBytes("D:/Documents/Projects/C#-DLL/DLL/DLL/obj/Debug/netstandard2.0/DLL.dll");
                assemblies[_assemblies.Length] = Assembly.Load(content);
            }
            Array.Copy(_assemblies, assemblies, _assemblies.Length);
        }
        StringBuilder builder = new StringBuilder(assemblies.Length * 256 * 16);
        
        Func<Assembly, bool> filterAssembly = assembly => string.Compare(GetName(assembly), 0, "Unity", 0, "Unity".Length) == 0;
        if (!writeToFolder)
            filterAssembly = assembly => string.Compare(GetName(assembly), 0, "Unity", 0, "Unity".Length) == 0 || GetName(assembly) == "DLL";
        
        NameCollisionData.Init(assemblies, 32768);
        
        List<System.Threading.Tasks.Task<string>> tasks = new List<System.Threading.Tasks.Task<string>>(assemblies.Length);
        var fileTasks = new System.Collections.Concurrent.ConcurrentBag<System.Threading.Tasks.Task>();
        
        static string GetName(Assembly assembly) => assembly.GetName().Name;
        
        static string GetAssemblyTitle(Assembly assembly, Type[] types)
        {
            return $"-------- {{{GetName(assembly)}({assembly.Location})}}: {types.Length - 1,4} --------\n";
        }
        
        static string AppendAssembly(Assembly assembly, StringBuilder builder, Action<Type[], StringBuilder> callback)
        {
            Type[] types = assembly.GetTypes();
            if (builder == null)
                builder = new StringBuilder(256 * 16);
            builder.Append(GetAssemblyTitle(assembly, types));
            callback(types, builder);
            return builder.ToString();
        }
        
        static void ForEach(int min, int max, Action<Tuple<int, int>> callback)
        {
            System.Threading.Tasks.Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(min, max), (range, state) => callback(range));
        }
        
        string AppendType(Assembly assembly, StringBuilder builder)
        {
            return AppendAssembly(assembly, builder, (types, builder) =>
                                  {
                                      foreach (Type type in types)
                                          AppendGlobalType(builder, type, GetGlobalType(type, debugNamespace));
                                  });
        }
        
        string AppendTypeWithTask(Assembly assembly, StringBuilder builder)
        {
            return AppendAssembly(assembly, builder, (types, builder) =>
                                  {
                                      System.Threading.Tasks.Task<string>[] tasks = new System.Threading.Tasks.Task<string>[types.Length];
                                      for (int i = 0; i < types.Length; ++i)
                                          tasks[i] = RunTask(i, i => GetGlobalType(types[i], debugNamespace));
                                      for (int i = 0; i < tasks.Length; ++i)
                                          AppendGlobalType(builder, types[i], tasks[i].Result);
                                  });
        }
        
        string AppendTypeWithPartition(Assembly assembly, StringBuilder builder)
        {
            return AppendAssembly(assembly, builder, (types, builder) =>
                                  {
                                      if (types.Length > 0)
                                      {
                                          string[] data = new string[types.Length];
                                          ForEach(0, types.Length, range =>
                                                  {
                                                      for (int i = range.Item1; i < range.Item2; i++)
                                                          data[i] = GetGlobalType(types[i], debugNamespace);
                                                  });
                                          for (int i = 0; i < data.Length; ++i)
                                              AppendGlobalType(builder, types[i], data[i]);
                                      }
                                  });
        }
        
        void IterateAssemblies(Action<Assembly> callback)
        {
            foreach (Assembly assembly in assemblies)
                if (filterAssembly(assembly))
                callback(assembly);
        }
        
        T[] IteratePartitionAssemblies<T>(Func<Assembly, T> callback)
        {
            T[] data = new T[assemblies.Length];
            ForEach(0, assemblies.Length, range =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                            if (filterAssembly(assemblies[i]))
                            data[i] = callback(assemblies[i]);
                    });
            return data;
        }
        
        void AppendGlobalType(StringBuilder builder, Type type, string data)
        {
            if (data != null)
            {
                if (writeToFolder)
                {
                    fileTasks.Add(RunTask(() =>
                                          {
                                              string path = "D:/Documents/Unity/Decompile/Temp/";
                                              if (type.Namespace != null)
                                                  path += type.Namespace + "/";
                                              System.IO.Directory.CreateDirectory(path);
                                              path += GetNameWithoutGeneric(type.Name);
                                              int genericCount = type.GetGenericArguments().Length;
                                              if (genericCount > 0)
                                                  path += "`" + genericCount;
                                              System.IO.File.WriteAllText(path + ".cs", data);
                                          }));
                }
                builder.Append(data);
            }
        }
        
        switch (threadType)
        {
            case ThreadType.None:
            IterateAssemblies(assembly => AppendType(assembly, builder));
            break;
            case ThreadType.Assembly:
            IterateAssemblies(assembly => tasks.Add(RunTask(() => AppendType(assembly, null))));
            break;
            case ThreadType.Type:
            IterateAssemblies(assembly => AppendTypeWithTask(assembly, builder));
            break;
            case ThreadType.Load:
            {
                var loadingTasks = new List<System.Threading.Tasks.Task<(Type[], Assembly)>>(assemblies.Length);
                IterateAssemblies(assembly => loadingTasks.Add(RunTask(() => (assembly.GetTypes(), assembly))));
                
                int typeCount = 0;
                foreach (var loadingTask in loadingTasks)
                    typeCount += loadingTask.Result.Item1.Length;
                tasks.Capacity += typeCount;
                
                foreach (var loadingTask in loadingTasks)
                {
                    (Type[] types, Assembly assembly) = loadingTask.Result;
                    
                    tasks.Add(RunTask(() => GetAssemblyTitle(assembly, types)));
                    foreach (Type _type in types)
                    {
                        Type type = _type;
                        tasks.Add(RunTask(() => GetGlobalType(type, debugNamespace)));
                    }
                }
            }
            break;
            case ThreadType.All:
            IterateAssemblies(assembly => tasks.Add(RunTask(() => AppendTypeWithTask(assembly, null))));
            break;
            case ThreadType.Partition_Assembly:
            {
                foreach (string data in IteratePartitionAssemblies(assembly => AppendType(assembly, null)))
                    builder.Append(data);
            }
            break;
            case ThreadType.Partition_Type:
            IterateAssemblies(assembly => tasks.Add(RunTask(() => AppendTypeWithPartition(assembly, null))));
            break;
            case ThreadType.Partion_Load:
            {
                Type[][] allTypes = IteratePartitionAssemblies(assembly => assembly.GetTypes());
                for (int i = 0; i < allTypes.Length; ++i)
                    if (allTypes[i] != null)
                    tasks.Add(RunTask(i, i => AppendTypeWithPartition(assemblies[i], null)));
            }
            break;
            case ThreadType.Partition_All:
            {
                foreach (string data in IteratePartitionAssemblies(assembly => AppendTypeWithPartition(assembly, null)))
                    builder.Append(data);
            }
            break;
        }
        
        foreach (var task in tasks)
            builder.Append(task.Result);
        System.Threading.Tasks.Task.WaitAll(fileTasks.ToArray());
        
        string message = "Search complete";
        UnityEngine.Object context = null;
        if (generateFile)
        {
            string path = "Assets/Files/types.txt";
            System.IO.File.WriteAllText(path, builder.ToString());
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            message = "File is created at " + path;
            context = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
        message += $" after {watch.ElapsedMilliseconds}ms";
        message += GameUtils.GetAllString(NameCollisionData.unusedAttributes, "\nUnused Attribtes: ");
        Debug.Log(message, context);
        UnityEngine.Profiling.Profiler.EndSample();
        
        static string GetGlobalType(Type type, bool debugNamespace)
        {
            string result = null;
            if (type.IsPublic)
            {
                NameCollisionData.ScopeCollision(type, data => AddAllTypes(data, type), data =>
                                                 {
                                                     StringBuilder builder = new StringBuilder(256);
                                                     if (data != null)
                                                     {
                                                         if (!debugNamespace)
                                                             data.currentNamespaces.Sort();
                                                         int i = 0;
                                                         foreach (string _namespace in data.currentNamespaces)
                                                         {
                                                             if (_namespace != type.Namespace && type.Namespace?.StartsWith(_namespace + ".") != true)
                                                             {
                                                                 builder.Append("using ").Append(_namespace);
                                                                 if (debugNamespace)
                                                                     builder.Append($"({data.debugNamespaces[i]} from {data.debugNamespaces[i].Assembly})");
                                                                 builder.Append(";\n");
                                                             }
                                                             ++i;
                                                         }
                                                         if (builder.Length != 0)
                                                             builder.Append('\n');
                                                     }
                                                     
                                                     int indent = type.Namespace != null ? 1 : 0;
                                                     if (indent != 0)
                                                         builder.Append("namespace ").Append(type.Namespace).Append("\n{\n");
                                                     AppendMemberDefinition(builder, type, indent);
                                                     if (indent != 0)
                                                         builder.Append("}\n");
                                                     
                                                     Debug.Assert(builder.Length != 0, type);
                                                     result = builder.ToString();
                                                 });
            }
            return result;
        }
        
        static void AddAllTypes(NameCollisionData database, MemberInfo member)
        {
            FieldInfo field = member as FieldInfo;
            PropertyInfo property = member as PropertyInfo;
            EventInfo eventInfo = member as EventInfo;
            Type type = member as Type;
            MethodBase method = member as MethodBase;
            bool isDelegate = type != null && typeof(Delegate).IsAssignableFrom(type);
            
            bool skip = false;
            switch (member.MemberType)
            {
                case MemberTypes.Method: skip = method.IsSpecialName && !member.Name.StartsWith("op_"); break;
                case MemberTypes.Field: skip = ((FieldInfo)member).IsSpecialName; break;
            }
            if (skip) return;
            
            AddMember(database, member, true);
            
            MethodInfo getter = null, setter = null;
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                    {
                        getter = property.GetGetMethod(true);
                        setter = property.GetSetMethod(true);
                        if (getter != null && setter != null)
                            method = GetMethodAccess(getter) <= GetMethodAccess(setter) ? getter : setter;
                        else
                            method = getter ?? setter;
                    }
                    break;
                    case MemberTypes.Event: method = eventInfo.GetAddMethod(true); break;
                    case MemberTypes.NestedType: goto case MemberTypes.TypeInfo;
                    case MemberTypes.TypeInfo:
                    {
                        if (isDelegate)
                            method = type.GetMethod("Invoke");
                    }
                    break;
                }
            }
            MethodInfo methodInfo = method as MethodInfo;
            
            AccessModifier access = AccessModifier.None;
            {
                if (type != null)
                {
                    // NOTE: Global types can only have public or internal modifier
                    if (!type.IsNested) access = type.IsPublic ? AccessModifier.Public : AccessModifier.Internal;
                    
                    else if (type.IsNestedPublic) access = AccessModifier.Public;
                    else if (type.IsNestedAssembly) access = AccessModifier.Internal;
                    else if (type.IsNestedPrivate) access = AccessModifier.Private;
                    
                    // NOTE: Struct types and members don't have the below modifiers
                    else if (type.IsNestedFamily) access = AccessModifier.Protected;
                    else if (type.IsNestedFamORAssem) access = AccessModifier.ProtectedInternal;
                    else if (type.IsNestedFamANDAssem) access = AccessModifier.PrivateProtected;
                }
                else if (field != null)
                {
                    if (false) { }
                    else if (field.IsPublic) access = AccessModifier.Public;
                    else if (field.IsFamilyOrAssembly) access = AccessModifier.ProtectedInternal;
                    else if (field.IsFamily) access = AccessModifier.Protected;
                    else if (field.IsAssembly) access = AccessModifier.Internal;
                    else if (field.IsFamilyAndAssembly) access = AccessModifier.PrivateProtected;
                    else if (field.IsPrivate) access = AccessModifier.Private;
                }
                else if (method != null)
                    access = GetMethodAccess(method);
                
                if (access > maxAccess)
                    return;
            }
            
            // Attributes
            AddAttributeTypes(database, member.GetCustomAttributesData());
            if (member.MemberType == MemberTypes.Method)
                AddAttributeTypes(database, methodInfo.ReturnParameter.GetCustomAttributesData());
            
            // Declare/return type
            {
                Type declareType = null;
                ParameterInfo returnParam = methodInfo?.ReturnParameter;
                switch (member.MemberType)
                {
                    case MemberTypes.Field: declareType = field.FieldType; break;
                    case MemberTypes.Event: declareType = eventInfo.EventHandlerType; break;
                    case MemberTypes.Property:
                    {
                        if (getter != null)
                            returnParam = getter.ReturnParameter;
                        else
                            declareType = property.PropertyType;
                    }
                    break;
                }
                
                if (returnParam != null && declareType == null)
                    declareType = returnParam.ParameterType;
                if (declareType != null)
                    AddType(database, declareType);
            }
            
            // Generic constraint + Parameters/Base Types-Members
            switch (member.MemberType)
            {
                case MemberTypes.Constructor: goto case MemberTypes.Method;
                case MemberTypes.TypeInfo: goto case MemberTypes.NestedType;
                
                case MemberTypes.Property: AddArrayType(property.GetIndexParameters(), AddParameter); break;
                case MemberTypes.Method: AddArrayType(method.GetParameters(), AddParameter); break;
                case MemberTypes.NestedType:
                {
                    if (isDelegate)
                        goto case MemberTypes.Method;
                    AddArrayType(GetBaseTypes(type), AddType);
                    foreach (MemberInfo mem in type.GetMembers(flags))
                        AddAllTypes(database, mem);
                }
                break;
                
                void AddArrayType<T>(T[] array, Action<NameCollisionData, T> each)
                {
                    Iterate(array, item => each(database, item));
                    // TODO: Move generic constraints to AddMember with isDecl
                    GetGenericConstraints(GetGenericArguments(member),
                                          (constraintTypes, parameter) => Iterate(constraintTypes, constraint =>
                                                                                  {
                                                                                      if (constraint != typeof(ValueType))
                                                                                          AddType(database, constraint);
                                                                                  }));
                }
                
                static void AddParameter(NameCollisionData data, ParameterInfo param)
                {
                    AddAttributeTypes(data, param.GetCustomAttributesData());
                    AddType(data, param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType);
                }
            }
            
            static void AddType(NameCollisionData database, Type type) => AddMember(database, type);
            
            static void AddMember(NameCollisionData database, MemberInfo member, bool isDecl = false)
            {
                Type type = member as Type;
                MethodInfo method = member as MethodInfo;
                
                if (method != null)
                {
                    if (!method.IsSpecialName)
                        Iterate(GetGenericArguments(member), type => AddGenericArgument(database, type, isDecl));
                    else if (member.Name == "op_Implicit") type = method.ReturnType;
                    else if (member.Name == "op_Explicit") type = method.ReturnType;
                }
                
                if (type != null)
                {
                    Type definitionType = GetElementType(type, out _, out _, out Type elementType);
                    if (!Array.Exists(ignoredTypes, name => definitionType.FullName == name) &&
                        GetPrimitiveTypeName(definitionType) == null)
                    {
                        Iterate(GetGenericArguments(elementType), type => AddGenericArgument(database, type, isDecl));
                        if (definitionType != typeof(Nullable<>) && !IsTuple(definitionType))
                        {
                            if (definitionType.Namespace != null && !NameCollisionData.CheckNamespace(definitionType.Namespace))
                            {
                                database.currentNamespaces.Add(definitionType.Namespace);
                                database.debugNamespaces.Add(definitionType);
                            }
                        }
                    }
                }
                
                static void AddGenericArgument(NameCollisionData database, Type type, bool isDecl)
                {
                    if (type.IsGenericParameter && isDecl)
                        AddAttributeTypes(database, type.GetCustomAttributesData());
                    else
                        AddMember(database, type, isDecl);
                }
            }
            
            static void Iterate<T>(IList<T> list, Action<T> action)
            {
                foreach (T item in list)
                    action(item);
            }
            
            static void AddAttributeTypes(NameCollisionData database, IList<CustomAttributeData> attributes)
            {
                Iterate(attributes, att => AddType(database, att.AttributeType));
                foreach (CustomAttributeData attribute in attributes)
                {
                    Iterate(GetAttributeArguments(attribute), arg =>
                            {
                                if (arg.Value != null)
                                {
                                    if (arg.ArgumentType == typeof(Type))
                                        AddType(database, (Type)arg.Value);
                                    else if (arg.Value is ReadOnlyCollection<CustomAttributeTypedArgument> argArray)
                                        if (argArray.Count > 0 && argArray[0].ArgumentType == typeof(Type))
                                        Iterate(argArray, item => AddType(database, (Type)item.Value));
                                }
                            });
                }
            }
        }
        
        // TODO: Cleanup and combine AddAllTypes and AppendMemberDefinition into one
        static void AppendMemberDefinition(StringBuilder _builder, MemberInfo member, int indent)
        {
            FieldInfo field = member as FieldInfo;
            PropertyInfo property = member as PropertyInfo;
            EventInfo eventInfo = member as EventInfo;
            Type type = member as Type;
            MethodBase method = member as MethodBase;
            
            bool skip = false;
            switch (member.MemberType)
            {
                case MemberTypes.Method: skip = method.IsSpecialName && !member.Name.StartsWith("op_"); break;
                case MemberTypes.Field: skip = ((FieldInfo)member).IsSpecialName; break;
                
                // NOTE: Can't know if an empty constructor was declared or auto-generated, VS also doesn't know
                // https://stackoverflow.com/questions/3190575/detect-compiler-generated-default-constructor-using-reflection-in-c-sharp
                case MemberTypes.Constructor: break;
            }
            if (skip) return;
            
            string name = GetMemberName(member, MemberNameMode.IsDecl);
            
            // https://codeblog.jonskeet.uk/2014/08/22/when-is-a-constant-not-a-constant-when-its-a-decimal/
            var decimalConst = field?.GetCustomAttribute<System.Runtime.CompilerServices.DecimalConstantAttribute>();
            bool isDelegate = type != null && typeof(Delegate).IsAssignableFrom(type);
            
            MethodInfo getter = null, setter = null;
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                    {
                        getter = property.GetGetMethod(true);
                        setter = property.GetSetMethod(true);
                        // NOTE:
                        // 1. Property's accessors can only have higher restrictive access level than the property
                        // 2. A property must have _both_ a getter and a setter so that _one_ of them can have a custom access modifier
                        // Which means a property has the lower access modify between the two accessors or it has the only access level 
                        if (getter != null && setter != null)
                            method = GetMethodAccess(getter) <= GetMethodAccess(setter) ? getter : setter;
                        else
                            method = getter ?? setter;
                        Debug.Assert(method != null, member.Name + " of " + member.DeclaringType.FullName);
                    }
                    break;
                    case MemberTypes.Event:
                    {
                        // NOTE: C# events always have both add and remove method, but never raise method
                        method = eventInfo.GetAddMethod(true);
                        Debug.Assert(eventInfo.GetRemoveMethod(true) != null && method != null, name);
                        Debug.Assert(eventInfo.GetRaiseMethod(true) == null, name + " " + eventInfo.GetRaiseMethod(true)?.Name);
                    }
                    break;
                    case MemberTypes.NestedType: goto case MemberTypes.TypeInfo;
                    case MemberTypes.TypeInfo:
                    {
                        if (isDelegate)
                            method = type.GetMethod("Invoke");
                    }
                    break;
                }
            }
            MethodInfo methodInfo = method as MethodInfo;
            
            AccessModifier access = AccessModifier.None;
            {
                if (type != null)
                {
                    // NOTE: Global types can only have public or internal modifier
                    if (!type.IsNested) access = type.IsPublic ? AccessModifier.Public : AccessModifier.Internal;
                    
                    else if (type.IsNestedPublic) access = AccessModifier.Public;
                    else if (type.IsNestedAssembly) access = AccessModifier.Internal;
                    else if (type.IsNestedPrivate) access = AccessModifier.Private;
                    
                    // NOTE: Struct types and members don't have the below modifiers
                    else if (type.IsNestedFamily) access = AccessModifier.Protected;
                    else if (type.IsNestedFamORAssem) access = AccessModifier.ProtectedInternal;
                    else if (type.IsNestedFamANDAssem) access = AccessModifier.PrivateProtected;
                }
                else if (field != null)
                {
                    if (false) { }
                    else if (field.IsPublic) access = AccessModifier.Public;
                    else if (field.IsFamilyOrAssembly) access = AccessModifier.ProtectedInternal;
                    else if (field.IsFamily) access = AccessModifier.Protected;
                    else if (field.IsAssembly) access = AccessModifier.Internal;
                    else if (field.IsFamilyAndAssembly) access = AccessModifier.PrivateProtected;
                    else if (field.IsPrivate) access = AccessModifier.Private;
                }
                else if (method != null)
                    access = GetMethodAccess(method);
                
                if (access > maxAccess)
                    return;
            }
            
            string attributes = null;
            {
                IList<CustomAttributeData> data = member.GetCustomAttributesData();
                IList<CustomAttributeData> returnAtts = new List<CustomAttributeData>();
                if (member.MemberType == MemberTypes.Method)
                    returnAtts = methodInfo.ReturnParameter.GetCustomAttributesData();
                
                StringBuilder attributeBuider = new StringBuilder((data.Count + returnAtts.Count) * 16);
                AppendAttributes(attributeBuider, data, indent);
                AppendAttributes(attributeBuider, returnAtts, indent, "return");
                if (type?.IsGenericParameter == true)
                    Debug.Log(type + ": " + data.Count);
                // NOTE: I can do the same for [field], [property], [event], and delegate's return param, but VS doesn't display those so why bother
                // The same goes for [assembly] and [module] attributes, but I never ever care about those
                attributes = attributeBuider.ToString();
                
                // TESTING
                NameCollisionData.GetUnusedAttributes(member, data);
            }
            
            // The abstract modifier can be used on a        class, method, property, indexer,                        or event
            // The static   modifier can be used on a field, class, method, property,          operator, constructor, or event
            // The virtual  modifier can be used on a               method, property, indexer,                        or event
            // The override modifier can be used on an inherited    method, property, indexer,                        or event
            // The sealed   modifier can be used on an inherited class or anything with an override modifier on it
            // const on fields
            // readonly on fields, structs, and parameters
            // ref on structs, parameters, and ref(readonly) returns on methods, properties, indexers, and delegates
            // event on delegates
            string modifier = null;
            {
                if (type != null)
                {
                    if (!type.IsInterface && !type.IsEnum && !isDelegate)
                    {
                        if (type.IsAbstract && type.IsSealed)
                            modifier = "static";
                        else if (type.IsAbstract) // NOTE: interfaces are also IsAbstract
                            modifier = "abstract";
                        else if (!type.IsValueType && type.IsSealed) // NOTE: value types (like struct and enum) are sealed by default
                            modifier = "sealed";
                        else
                        {
                            if (HasAttribute(type.CustomAttributes, readonlyAttribute))
                                modifier = "readonly";
                            if (HasAttribute(type.CustomAttributes, refAttribute))
                                modifier += (modifier != null ? " " : "") + "ref";
                        }
                    }
                }
                else if (field != null)
                {
                    // NOTE: VS doesn't handle "unsafe fixed" array
                    if (field.IsLiteral || decimalConst != null)
                        modifier = "const";
                    else if (field.IsInitOnly && field.IsStatic)
                        modifier = "static readonly";
                    else if (field.IsInitOnly)
                        modifier = "readonly";
                    else if (field.IsStatic)
                        modifier = "static";
                }
                else if (method != null)
                {
                    // virtual:   isVirtual !isFinal (because interface is virtual-final)
                    // override:  isVirtual baseMethod != method (sealed: IsFinal)
                    // abstract:  isVirtual isAbstract
                    // static:    isStatic
                    // NOTE: VS doesn't handle the new modifier
                    if (method.IsStatic)
                        modifier = "static";
                    if (method.IsVirtual)
                    {
                        if (IsMethodOverride(methodInfo))
                            modifier = method.IsFinal ? "sealed override" : "override";
                        else if (method.IsAbstract)
                            modifier = "abstract";
                        else if (!method.IsFinal)
                            modifier = "virtual";
                    }
                }
            }
            
            string declare = null;
            {
                Type declareType = null;
                ParameterInfo returnParam = methodInfo?.ReturnParameter;
                switch (member.MemberType)
                {
                    case MemberTypes.Field: declareType = field.FieldType; break;
                    case MemberTypes.Event:
                    {
                        declare = "event ";
                        declareType = eventInfo.EventHandlerType;
                    }
                    break;
                    case MemberTypes.Property:
                    {
                        if (getter != null)
                            returnParam = getter.ReturnParameter;
                        else
                            declareType = property.PropertyType;
                    }
                    break;
                    case MemberTypes.Method:
                    {
                        if (method.IsSpecialName && (method.Name == "op_Implicit" || method.Name == "op_Explicit"))
                        {
                            declare = (method.Name == "op_Implicit" ? "implicit" : "explicit") + " operator";
                            returnParam = null;
                        }
                    }
                    break;
                    case MemberTypes.TypeInfo: goto case MemberTypes.NestedType;
                    case MemberTypes.NestedType:
                    {
                        if (isDelegate)
                            declare = "delegate ";
                        else if (type.IsClass)
                            declare = "class";
                        else if (type.IsInterface)
                            declare = "interface";
                        else if (type.IsEnum)
                            declare = "enum";
                        else if (type.IsValueType)
                            declare = "struct";
                    }
                    break;
                }
                
                if (returnParam != null && declareType == null)
                {
                    declare += GetParameterModifier(returnParam, returnParam.GetCustomAttributesData());
                    declareType = returnParam.ParameterType;
                }
                
                if (declareType != null)
                    declare += GetMemberName(declareType, tupleNames: GetTupleNames((ICustomAttributeProvider)field ?? returnParam));
            }
            
            string value = null;
            {
                StringBuilder valueBuilder = new StringBuilder(256);
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                    {
                        if (modifier == "const")
                            AppendDefaultValue(valueBuilder.Append(" = "), field.FieldType,
                                               decimalConst != null ? decimalConst.Value : field.GetRawConstantValue());
                    }
                    break;
                    case MemberTypes.Property:
                    {
                        AppendArrayValue(property.GetIndexParameters(), "[", "]", AppendParameter);
                        valueBuilder.Append(" {");
                        AccessModifier methodAccess = GetMethodAccess(method);
                        AppendAccessor(getter, "get");
                        AppendAccessor(setter, "set");
                        valueBuilder.Append(" }");
                        
                        void AppendAccessor(MethodInfo accessorMethod, string accessorName)
                        {
                            if (accessorMethod != null)
                            {
                                AccessModifier _access = GetMethodAccess(accessorMethod);
                                if (_access <= maxAccess)
                                {
                                    valueBuilder.Append(' ');
                                    if (_access > methodAccess)
                                        AppendAccessModifier(valueBuilder, _access);
                                    valueBuilder.Append(accessorName).Append(';');
                                }
                            }
                        }
                    }
                    break;
                    
                    case MemberTypes.Constructor: goto case MemberTypes.Method;
                    case MemberTypes.Method:
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length == 0)
                            valueBuilder.Append("()");
                        AppendArrayValue(parameters, "(", ")", AppendParameter);
                    }
                    break;
                    
                    case MemberTypes.TypeInfo: goto case MemberTypes.NestedType;
                    case MemberTypes.NestedType:
                    {
                        if (isDelegate)
                            goto case MemberTypes.Method;
                        
                        AppendArrayValue(GetBaseTypes(type), " : ", null, (builder, baseType) =>
                                         AppendMemberName(builder, baseType, baseType.IsInterface ? null : type, MemberNameMode.BaseType));
                        
                        valueBuilder.Append('\n');
                        valueBuilder.AppendIndentLine("{", indent);
                        
                        // NOTE: Group order (newline): fields -> constructors -> finalizers -> properties -> events -> methods
                        // -> operators -> conversion operators -> nested types -> delegates
                        // TODO: Element order (no newline): const -> static readonly -> static -> readonly, indexers -> properties
                        Func<MemberInfo, bool>[] groupTable = new Func<MemberInfo, bool>[]
                        {
                            member => member.MemberType == MemberTypes.Field,
                            member => member.MemberType == MemberTypes.Constructor,
                            member => member.MemberType == MemberTypes.Method && member.Name == "Finalize",
                            member => member.MemberType == MemberTypes.Property &&  IsIndexer((PropertyInfo)member),
                            member => member.MemberType == MemberTypes.Property && !IsIndexer((PropertyInfo)member),
                            member => member.MemberType == MemberTypes.Event,
                            member => IsMethodSpecial(member, special: false, equal: false, "Finalize"),
                            member => IsMethodSpecial(member, special: true , equal: false, "op_Implicit", "op_Explicit"),
                            member => IsMethodSpecial(member, special: true , equal: true , "op_Implicit", "op_Explicit"),
                            member => member.MemberType == MemberTypes.NestedType
                        };
                        int noBlankLineIndex = 4; // NOTE: This is for removing blank line between indexers and properties
                        
                        static bool IsMethodSpecial(MemberInfo member, bool special, bool equal, params string[] names)
                        {
                            MethodInfo method = member as MethodInfo;
                            if (method == null || method.IsSpecialName != special)
                                return false;
                            
                            foreach (string name in names)
                                if (name == method.Name)
                                return equal;
                            return !equal;
                        }
                        
                        // NOTE: After some profiling, iterating through the array multiple times is slightly faster than sorting
                        MemberInfo[] members = type.GetMembers(flags);
                        bool isPreviousEmpty = true;
                        int oldIndex = -1;
                        for (int i = 0; i < groupTable.Length; ++i)
                        {
                            bool isEmpty = true;
                            foreach (MemberInfo mem in members)
                            {
                                bool success = groupTable[i](mem);
                                if (success)
                                {
                                    int length = valueBuilder.Length;
                                    Type currentType = mem.MemberType == MemberTypes.NestedType ? (Type)mem : type;
                                    NameCollisionData.ScopeType(currentType, () => AppendMemberDefinition(valueBuilder, mem, indent + 1));
                                    if (isEmpty && valueBuilder.Length > length)
                                    {
                                        if (!isPreviousEmpty && !(noBlankLineIndex == i && oldIndex == i - 1))
                                            valueBuilder.Insert(length, '\n');
                                        isPreviousEmpty = isEmpty = false;
                                        oldIndex = i;
                                    }
                                }
                            }
                        }
                        
                        valueBuilder.AppendIndent("}", indent);
                    }
                    break;
                }
                
                value = valueBuilder.ToString();
                
                void AppendArrayValue<T>(T[] array, string start, string end, Action<StringBuilder, T> each)
                {
                    AppendArray(valueBuilder, array, start, end, element => each(valueBuilder, element));
                    GetGenericConstraints(GetGenericArguments(member), (constraintTypes, parameter) =>
                                          {
                                              List<string> constraints = new List<string>(constraintTypes.Length + 2);
                                              bool hasStruct = false;
                                              bool hasUnmanaged = HasAttribute(parameter.CustomAttributes, unmanagedAttribute);
                                              
                                              foreach (Type constraint in constraintTypes)
                                              {
                                                  if (constraint == typeof(ValueType))
                                                      hasStruct = true;
                                                  else
                                                      constraints.Add(GetMemberName(constraint));
                                              }
                                              
                                              // NOTE: class, struct, unmanaged, notnull, and default can't be combined (C# 9 has `default` constraint)
                                              // unmanaged is also struct, struct is also new() and NotNullableValueTypeConstraint
                                              // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters
                                              if (hasUnmanaged)
                                                  constraints.Add("unmanaged");
                                              else if (hasStruct)
                                                  constraints.Add("struct");
                                              else
                                              {
                                                  GenericParameterAttributes attribute = parameter.GenericParameterAttributes;
                                                  if ((attribute & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                                                      constraints.Add("class");
                                                  if ((attribute & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                                                      constraints.Add("new()");
                                              }
                                              
                                              // NOTE: This doesn't handle the `notnull` constraint
                                              // because the documentation is fuzzy and I don't ever use nor care about nullable context
                                              
                                              AppendArray(valueBuilder, constraints, $" where {parameter.Name} : ", null, str => valueBuilder.Append(str));
                                          });
                }
            }
            
            StringBuilder builder = _builder;
            
            builder.Append(attributes);
            builder.AppendIndent(null, indent);
            bool isFinalizer = name[0] == '~';
            Type parentType = member.DeclaringType;
            bool isParentEnum = parentType?.IsEnum == true;
            if (parentType?.IsInterface != true && !isParentEnum && !isFinalizer)
            {
                AppendAccessModifier(builder, access);
                if (modifier != null)
                    builder.Append(modifier).Append(' ');
            }
            
            if (!isParentEnum && !isFinalizer && declare != null)
                builder.Append(declare).Append(' ');
            
            builder.Append(name);
            if (value != null)
                builder.Append(value);
            
            if (member.MemberType != MemberTypes.Property && (type == null || isDelegate))
            {
                if (!isParentEnum)
                    builder.Append(';');
                else if (parentType.GetFields(flags).Last() != member) // TODO: Check if the fields order for enum is well-defined
                    builder.Append(',');
            }
            builder.Append('\n');
            
            static void AppendAccessModifier(StringBuilder builder, AccessModifier access) => builder.Append(access.CamelCase().ToLower()).Append(' ');
        }
        
        static AccessModifier GetMethodAccess(MethodBase method)
        {
            if (method.IsPublic) return AccessModifier.Public;
            if (method.IsFamilyOrAssembly) return AccessModifier.ProtectedInternal;
            if (method.IsFamily) return AccessModifier.Protected;
            if (method.IsAssembly) return AccessModifier.Internal;
            if (method.IsFamilyAndAssembly) return AccessModifier.PrivateProtected;
            if (method.IsPrivate) return AccessModifier.Private;
            return AccessModifier.None;
        }
        static string GetPrimitiveTypeName(Type type)
        {
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(long)) return "long";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(char)) return "char";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(double)) return "double";
            if (type == typeof(float)) return "float";
            if (type == typeof(string)) return "string";
            if (type == typeof(object)) return "object";
            if (type == typeof(void)) return "void";
            return null;
        }
        static string GetNameWithoutGeneric(string name)
        {
            int index = name.IndexOf('`');
            if (index != -1)
                name = name.Substring(0, index);
            return name;
        }
        
        static Type GetElementType(Type type, out int arrayCount, out int pointerCount, out Type elementType)
        {
            if (type.IsByRef)
                type = type.GetElementType();
            
            // NOTE: int*[] is valid, while int[]* is not
            for (arrayCount = 0; type.IsArray; ++arrayCount)
                type = type.GetElementType();
            for (pointerCount = 0; type.IsPointer; ++pointerCount)
                type = type.GetElementType();
            
            elementType = type;
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();
            return type;
        }
        static Type[] GetBaseTypes(Type type)
        {
            Type[] interfaces = type.IsEnum ? new Type[0] : type.GetInterfaces();
            bool hasBaseType = type.BaseType != null && type.BaseType != typeof(object) && !type.IsValueType;
            List<Type> types = new List<Type>(interfaces.Length + (hasBaseType ? 1 : 0));
            
            if (hasBaseType)
                types.Add(type.BaseType);
            
            Type[] baseInterfaces = hasBaseType ? type.BaseType.GetInterfaces() : new Type[0];
            Func<Type, bool> match = itf => !Array.Exists(baseInterfaces, t => t == itf);
            Array.Sort(baseInterfaces, (a, b) => string.Compare(a.Name, b.Name));
            
            // NOTE: This only display "implemented" interfaces, not "declared" interfaces
            // Because of that, it won't work for inherit/empty interfaces and inherit classes that don't override the interface's members
            // Afaik, C# reflection isn't powerful enough to do that
            // https://stackoverflow.com/questions/9793242/type-getinterfaces-for-declared-interfaces-only
            // VS also doesn't handle the interface inheritance case
            /*if (!type.IsInterface)
                match = itf => Array.Exists(type.GetInterfaceMap(itf).TargetMethods,
                        method => method.DeclaringType == type && !(method.IsVirtual && method.GetBaseDefinition() != method));*/
            
            foreach (Type itf in interfaces)
                if (match(itf))
                types.Add(itf);
            
            return types.ToArray();
        }
        static Type[] GetGenericArguments(MemberInfo member)
        {
            Type[] args = new Type[0];
            
            Type type = member as Type;
            if (type?.IsGenericType == true)
            {
                // https://stackoverflow.com/questions/19503905/type-generictypearguments-property-vs-type-getgenericarguments-method
                args = type.GetGenericArguments();
                if (type.IsNested)
                {
                    // NOTE: Afaik, the outer class's arguments will all be before the nested class's arguments (this might be UB)
                    Type[] parentArgs = type.DeclaringType.GetGenericArguments();
                    Type[] nestedArgs = new Type[args.Length - parentArgs.Length];
                    Array.Copy(args, parentArgs.Length, nestedArgs, 0, nestedArgs.Length);
                    args = nestedArgs;
                }
            }
            
            // void DoA<T>() { }             definition-generic-containsParameters
            // DoA<int>();                   generic (but nested members will never have this case)
            // class A<T> { void DoA() { } } containsParameters (The Invoke method of a delegate type is this case)
            // Note that the GetGenericArguments will never return T for A<T>.DoA()
            // https://stackoverflow.com/questions/34247315/c-sharp-reflection-how-to-determine-if-parameterinfo-is-a-generic-type-defined
            MethodBase method = member as MethodBase;
            if (method?.IsGenericMethod == true)
                args = method.GetGenericArguments();
            
            return args;
        }
        static Type[][] GetGenericConstraints(Type[] parameters, Action<Type[], Type> callback = null)
        {
            Type[][] constraints = new Type[parameters.Length][];
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i].IsGenericParameter)
                {
                    constraints[i] = parameters[i].GetGenericParameterConstraints();
                    callback?.Invoke(constraints[i], parameters[i]);
                }
            }
            return constraints;
        }
        
        static string GetMemberName(MemberInfo member, MemberNameMode mode = MemberNameMode.Default, IList<string> tupleNames = null)
        {
            string name = member.Name;
            Type type = member as Type;
            
            Debug.Assert(mode == MemberNameMode.IsDecl || type != null, member + ": " + mode);
            
            // https://stackoverflow.com/questions/19788010/which-c-sharp-type-names-are-special
            switch (member.MemberType)
            {
                case MemberTypes.Constructor: name = GetNameWithoutGeneric(member.DeclaringType.Name); break;
                case MemberTypes.Method:
                {
                    MethodBase method = (MethodBase)member;
                    if (!method.IsSpecialName)
                    {
                        if (method.Name == "Finalize" && IsMethodOverride((MethodInfo)method))
                            name = "~" + GetNameWithoutGeneric(member.DeclaringType.Name);
                        else if (method.IsGenericMethod) // NOTE: operators, accessors, finalizer, and constructors can't be generic
                            name = GetGenericMethodName(method);
                    }
                    
                    // https://stackoverflow.com/questions/11113259/how-to-call-custom-operator-with-reflection
                    else if (member.Name == "op_UnaryPlus") name = "operator +";
                    else if (member.Name == "op_UnaryNegation") name = "operator -";
                    else if (member.Name == "op_Increment") name = "operator ++";
                    else if (member.Name == "op_Decrement") name = "operator --";
                    else if (member.Name == "op_LogicalNot") name = "operator !";
                    else if (member.Name == "op_Addition") name = "operator +";
                    else if (member.Name == "op_Subtraction") name = "operator -";
                    else if (member.Name == "op_Multiply") name = "operator *";
                    else if (member.Name == "op_Division") name = "operator /";
                    else if (member.Name == "op_BitwiseAnd") name = "operator &";
                    else if (member.Name == "op_BitwiseOr") name = "operator |";
                    else if (member.Name == "op_ExclusiveOr") name = "operator ^";
                    else if (member.Name == "op_OnesComplement") name = "operator ~";
                    else if (member.Name == "op_Equality") name = "operator ==";
                    else if (member.Name == "op_Inequality") name = "operator !=";
                    else if (member.Name == "op_LessThan") name = "operator <";
                    else if (member.Name == "op_GreaterThan") name = "operator >";
                    else if (member.Name == "op_LessThanOrEqual") name = "operator <=";
                    else if (member.Name == "op_GreaterThanOrEqual") name = "operator >=";
                    else if (member.Name == "op_LeftShift") name = "operator <<";
                    else if (member.Name == "op_RightShift") name = "operator >>";
                    else if (member.Name == "op_Modulus") name = "operator %";
                    else if (member.Name == "op_True") name = "operator true";
                    else if (member.Name == "op_False") name = "operator false";
                    
                    else if (member.Name == "op_Implicit" || member.Name == "op_Explicit")
                    {
                        type = ((MethodInfo)method).ReturnType;
                        mode = MemberNameMode.Default;
                    }
                }
                break;
                case MemberTypes.Property:
                {
                    // NOTE: For some reason, C# doesn't mark a indexer name as special, even thought it's always "Item"
                    if (IsIndexer((PropertyInfo)member))
                        name = "this";
                }
                break;
            }
            
            if (type != null) // NOTE: Only true for conversion operators and (nested)types
            {
                Debug.Assert(name == member.Name, "Name: " + name + " Member: " + member.Name + " Type: " + type);
                Type definitionType = GetElementType(type, out int arrayCount, out int pointerCount, out Type elementType);
                
                if (mode == MemberNameMode.IsDecl)
                {
                    if (elementType.IsGenericType)
                        name = GetGenericTypeName(elementType, name);
                    else
                        name = elementType.Name;
                }
                else if ((name = GetPrimitiveTypeName(elementType)) == null)
                {
                    if (definitionType == typeof(Nullable<>))
                        name = GetNullabeTypeName(elementType);
                    else if (IsTuple(definitionType))
                        name = GetTupleTypeName(elementType);
                    else
                    {
                        name = elementType.Name;
                        
                        if (NameCollisionData.Contains(definitionType) == true)
                            name = HandleNameCollision(elementType.GetGenericArguments(), definitionType, mode == MemberNameMode.BaseType);
                        
                        // NOTE: Currently, mode only equals to NoAttributePostfix when the type is an attribute
                        // If I ever run this codepath in other cases, I need to check if the type is an attribute here
                        if (mode == MemberNameMode.NoAttributePostfix && name.EndsWith("Attribute"))
                            name = name.Substring(0, name.Length - "Attribute".Length);
                        else if (elementType.IsGenericType)
                            name = GetGenericTypeName(elementType, name);
                    }
                }
                
                if (pointerCount > 0 || arrayCount > 0)
                {
                    StringBuilder builder = new StringBuilder(name, name.Length + "*".Length * pointerCount + "[]".Length * arrayCount);
                    for (int i = 0; i < pointerCount; ++i)
                        builder.Append("*");
                    for (int i = 0; i < arrayCount; ++i)
                        builder.Append("[]");
                    name = builder.ToString();
                }
            }
            
            return name;
            
            static string HandleNameCollision(Type[] currentArgs, Type definitionType, bool isBaseType)
            {
                string name = definitionType.FullName.Replace('+', '.');
                Type targetType = NameCollisionData.GetCurrentType();
                Type[] targetArgs = targetType.GetGenericArguments();
                
                // NOTE: The base types of the current class are searched from the outer class first,
                // while all its members (including its attributes) start from itself.
                // class A
                // {
                //     [SomeAttribute(typeof(A))] // This A is the nested A
                //     class B : A // This A is the outer A
                //     {
                //         class A { }
                //     }
                // }
                // So set the target type to its parent here (if it's nested) AFTER getting the correct arguments
                if (isBaseType && targetType.IsNested)
                    targetType = targetType.DeclaringType;
                
                definitionType = GetSharedParentType(definitionType, targetType, out Type sharedParent);
                if (sharedParent != null)
                {
                    int argLength = sharedParent.GetGenericArguments().Length;
                    for (int endIndex = argLength; endIndex > 0; sharedParent = sharedParent.DeclaringType)
                    {
                        int startIndex = endIndex - GetGenericArguments(sharedParent).Length;
                        for (int i = startIndex; i < endIndex && definitionType != sharedParent; ++i)
                            if (currentArgs[i] != targetArgs[i])
                            definitionType = sharedParent;
                        endIndex = startIndex;
                    }
                }
                
                for (; definitionType != null; definitionType = definitionType.DeclaringType)
                {
                    if (!HasNameCollision(definitionType, targetType))
                    {
                        Debug.Assert(definitionType.FullName != null, definitionType);
                        name = name.Substring(definitionType.FullName.Length - definitionType.Name.Length);
                        break;
                    }
                }
                
                return name;
                
                static Type GetSharedParentType(Type type, Type currentType, out Type parentType)
                {
                    Type result = type;
                    int i = 0;
                    string[] names = type.FullName.Substring((type.Namespace?.Length ?? -1) + 1).Split('+');
                    
                    if (type.Namespace == currentType.Namespace)
                    {
                        string[] currentNames = currentType.FullName.Substring((type.Namespace?.Length ?? -1) + 1).Split('+');
                        int length = Mathf.Min(names.Length, currentNames.Length);
                        for (; i < length; ++i)
                            if (names[i] != currentNames[i])
                            break;
                    }
                    
                    // NOTE: result will be null if i == 0 (when the two types don't share the same parent)
                    Type oneBeforeFinalType = null;
                    for (; i < names.Length; ++i)
                    {
                        if (i == names.Length - 1)
                            oneBeforeFinalType = result;
                        result = result.DeclaringType;
                    }
                    
                    parentType = result;
                    if (result != type)
                        result = oneBeforeFinalType;
                    
                    Debug.Assert(result != null);
                    return result;
                }
                
                static bool HasNameCollision(Type type, Type targetType)
                {
                    bool collide = false;
                    int currentDistance = GetDistanceScore(type, targetType);
                    int score = GetNamespaceScore(type, targetType);
                    Debug.Assert(targetType != null, "Target type is null for " + type);
                    const int failedDistanceValue = 1 << 16; // Some high number
                    
                    if (currentDistance != 0)
                    {
                        foreach (Type testType in NameCollisionData.GetCollideTypes(type.Name))
                        {
                            // NOTE: Sometimes the types are the same but was referenced from different assemblies so check for fullname instead
                            if (testType.FullName == type.FullName) continue;
                            
                            if (NameCollisionData.CheckNamespace(testType.Namespace, true))
                            {
                                int testDistance = GetDistanceScore(testType, targetType);
                                if (testDistance < currentDistance) collide = true;
                                else if (testDistance == currentDistance)
                                {
                                    Debug.Assert(testDistance == failedDistanceValue,
                                                 $"{type.FullName}: {currentDistance}, {testType.FullName}, Target: {targetType}");
                                    if (!testType.IsNested)
                                    {
                                        int testScore = GetNamespaceScore(testType, targetType);
                                        if (testScore >= score)
                                            collide = true;
                                        Debug.Assert(score != testScore || score == 0,
                                                     $"{type.FullName}: {score}, {testType.FullName}: {testScore}, Target: {targetType}");
                                    }
                                }
                            }
                            
                            if (collide)
                                break;
                        }
                    }
                    
                    return collide;
                    
                    static int GetDistanceScore(Type type, Type targetType)
                    {
                        for (int i = 0; targetType != null; ++i, targetType = targetType.DeclaringType)
                            if (targetType == type || targetType == type.DeclaringType)
                            return i;
                        return failedDistanceValue;
                    }
                    
                    static int GetNamespaceScore(Type type, Type targetType)
                    {
                        int score = 0;
                        //if (targetType.Namespace == null) Debug.Log(targetType.Name);
                        //if (type.Namespace == null)       Debug.Log(type);
                        string[] namespaces = type.Namespace?.Split('.');
                        string[] targetNamespaces = targetType.Namespace?.Split('.');
                        if (namespaces != null && targetNamespaces != null && namespaces.Length <= targetNamespaces.Length)
                            for (; score < namespaces.Length; ++score)
                            if (namespaces[score] != targetNamespaces[score])
                            return 0;
                        return score;
                    }
                }
            }
            
            static string GetGenericName(IList<Type> args, string name, Action<StringBuilder, Type> action, string open = "<", string close = ">")
            {
                StringBuilder builder = new StringBuilder(name);
                AppendArray(builder, args, open, close, type => action(builder, type));
                return builder.ToString();
            }
            
            string GetGenericMethodName(MethodBase method) => GetGenericName(GetGenericArguments(method), method.Name,
                                                                             (builder, parameter) => AppendGenericParameter(builder, parameter, tupleNames));
            
            string GetNullabeTypeName(Type type) => GetMemberName(GetGenericArguments(type)[0], mode) + "?";
            
            string GetTupleTypeName(Type type)
            {
                Action<StringBuilder, Type> action = (sb, t) => AppendMemberName(sb, t);
                if (tupleNames != null && tupleNames.Count > 0)
                {
                    int currentCount = GetGenericArguments(type).Length;
                    IList<string> currentTupleNames = tupleNames.GetRange(0, currentCount);
                    IList<string> nestedTupleNames = tupleNames.GetRange(currentCount, tupleNames.Count - currentCount);
                    int parameterIndex = 0;
                    int argumentIndex = 0;
                    action = (builder, parameter) =>
                    {
                        int count = GetTupleCount(new Type[] { parameter });
                        IList<string> names = nestedTupleNames.GetRange(argumentIndex, count);
                        AppendTupleName(builder, parameter, names);
                        argumentIndex += count;
                        
                        if (currentTupleNames[parameterIndex] != null)
                            builder.Append(' ').Append(currentTupleNames[parameterIndex]);
                        ++parameterIndex;
                    };
                }
                return GetGenericName(GetGenericArguments(type), null, action, "(", ")");
            }
            
            string GetGenericTypeName(Type type, string name)
            {
                string[] names = name.Split('.');
                Type[] genericArgs = type.GetGenericArguments();
                
                int i = names.Length - 1;
                int genericIndex = genericArgs.Length;
                int tupleIndex = tupleNames?.Count ?? 0;
                
                for (Type parent = type; i >= 0 && genericIndex >= 0; parent = parent.DeclaringType, --i)
                {
                    if (parent == null)
                    {
                        Debug.LogError(GameUtils.GetAllString(names, $"{type}: {name} at index {i}, generic {genericIndex}"));
                        //break;
                    }
                    ArraySegment<Type> args = (ArraySegment<Type>)genericArgs.AdvanceRange(ref genericIndex, GetGenericArguments(parent).Length);
                    IList<string> typeTupleNames = tupleIndex > 0 ? tupleNames.AdvanceRange(ref tupleIndex, GetTupleCount(args)) : null;
                    
                    int parameterTupleIndex = 0;
                    Action<StringBuilder, Type> action = (builder, parameter) =>
                    {
                        IList<string> parameterTupleNames = null;
                        if ((typeTupleNames?.Count ?? 0) > 0)
                            parameterTupleNames = typeTupleNames.AdvanceRange(ref parameterTupleIndex, GetTupleCount(new Type[] { parameter }), true);
                        AppendGenericParameter(builder, parameter, parameterTupleNames);
                    };
                    
                    if (mode == MemberNameMode.NoGenericParameter)
                        action = (sb, t) => { if (!t.IsGenericParameter) AppendMemberName(sb, t); };
                    
                    // NOTE: Generic methods never have ` in their names, but generic types do (probably because of method overloading)
                    names[i] = GetGenericName(args, GetNameWithoutGeneric(names[i]), action);
                }
                return string.Join(".", names);
            }
            
            static int GetTupleCount(IList<Type> genericArgs)
            {
                int count = 0;
                foreach (Type arg in genericArgs)
                {
                    GetElementType(arg, out _, out _, out Type elementType);
                    Type[] args = elementType.GetGenericArguments();
                    if (IsTuple(elementType))
                        count += args.Length;
                    count += GetTupleCount(args);
                }
                return count;
            }
            
            void AppendGenericParameter(StringBuilder builder, Type type, IList<string> tupleNames)
            {
                if (mode == MemberNameMode.IsDecl && type.IsGenericParameter)
                    if (AppendAttributes(builder, type.GetCustomAttributesData(), -1))
                    builder.Append(' ');
                AppendTupleName(builder, type, tupleNames, mode);
            }
        }
        static string GetParameterModifier(ParameterInfo param, IList<CustomAttributeData> attributes)
        {
            string modifier = null;
            if (HasAttribute(attributes, paramAttribute))
                modifier = "params ";
            else if (param.IsOut)
                modifier = "out ";
            else if (param.IsIn)
                modifier = "in ";
            else if (param.ParameterType.IsByRef)
                modifier = "ref " + (HasAttribute(attributes, readonlyAttribute) ? "readonly " : "");
            
            if (HasAttribute(param.Member.CustomAttributes, extensionAttribute) && param.Position == 0)
                modifier += "this ";
            return modifier;
        }
        static List<CustomAttributeTypedArgument> GetAttributeArguments(CustomAttributeData attribute)
        {
            List<CustomAttributeTypedArgument> result = new List<CustomAttributeTypedArgument>(attribute.ConstructorArguments);
            result.AddRange(attribute.NamedArguments, arg => arg.TypedValue);
            return result;
        }
        static IList<string> GetTupleNames(ICustomAttributeProvider provider)
        {
            return provider?.GetCustomAttribute<System.Runtime.CompilerServices.TupleElementNamesAttribute>()?.TransformNames;
        }
        
        static void AppendMemberName(StringBuilder builder, Type type, ICustomAttributeProvider provider = null,
                                     MemberNameMode mode = MemberNameMode.Default) => AppendTupleName(builder, type, GetTupleNames(provider), mode);
        static void AppendTupleName(StringBuilder builder, Type type, IList<string> tupleNames, MemberNameMode mode = MemberNameMode.Default) =>
            builder.Append(GetMemberName(type, mode, tupleNames));
        
        static void AppendParameter(StringBuilder builder, ParameterInfo param)
        {
            IList<CustomAttributeData> attributes = param.GetCustomAttributesData();
            if (AppendAttributes(builder, attributes, -1))
                builder.Append(' ');
            builder.Append(GetParameterModifier(param, attributes));
            
            Type paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
            AppendMemberName(builder, paramType, param);
            builder.Append(' ').Append(param.Name);
            if (param.HasDefaultValue)
                AppendDefaultValue(builder.Append(" = "), paramType, param.RawDefaultValue);
        }
        static bool AppendAttributes(StringBuilder builder, IList<CustomAttributeData> data, int indent, string target = null)
        {
            string terminator = "\n";
            if (indent == -1)
            {
                indent = 0;
                terminator = null;
            }
            
            bool hasAtt = false;
            foreach (CustomAttributeData attribute in data)
            {
                if (Array.Exists(ignoredTypes, ignore => ignore == attribute.AttributeType.FullName))
                    continue;
                
                builder.AppendIndent("[", indent);
                if (target != null)
                    builder.Append(target).Append(": ");
                builder.Append(GetMemberName(attribute.AttributeType, MemberNameMode.NoAttributePostfix));
                
                List<CustomAttributeTypedArgument> arguments = GetAttributeArguments(attribute);
                int constructorCount = attribute.ConstructorArguments.Count;
                int index = 0;
                AppendArray(builder, arguments, "(", ")", arg =>
                            {
                                if (index >= constructorCount)
                                    builder.Append(attribute.NamedArguments[index - constructorCount].MemberName).Append(" = ");
                                AppendDefaultValue(builder, arg.ArgumentType, arg.Value);
                                ++index;
                            });
                
                builder.Append(']').Append(terminator);
                hasAtt = true;
            }
            
            return hasAtt;
        }
        static void AppendDefaultValue(StringBuilder builder, Type valueType, object value)
        {
            // NOTE: Constant fields and parameters can only have a default value for string (ref type) or builtin primitive types (value type)
            // Attributes are also like that but can have a System.Type, object, and Single-dimensional arrays of those types
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/attributes#2124-attribute-parameter-types
            if (value != null)
            {
                string str = value.ToString();
                // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.customattributetypedargument.value?view=net-7.0
                if (value is ReadOnlyCollection<CustomAttributeTypedArgument> argArray)
                {
                    builder.Append($"new[] {{");
                                                                                                                                            AppendArray(builder, argArray, null, null, arg => AppendDefaultValue(builder, arg.ArgumentType, arg.Value));
                                                                                                                                            builder.Append(" }");
                }
                else if (valueType == typeof(string))
                    builder.Append($"\"{GameUtils.EscapeString(str, "\'")}\"");
                else if (valueType == typeof(char))
                    builder.Append($"\'{GameUtils.EscapeString(str, "\"")}\'");
                else if (valueType == typeof(Type))
                    builder.Append($"typeof({GetMemberName((Type)value, MemberNameMode.NoGenericParameter)})");
                else
                    builder.Append(str);
                
                float? flt = null;
                try { flt = Convert.ToSingle(value); } catch (Exception) { }
                
                if (flt == null || Math.Abs((float)flt % 1) <= float.Epsilon) { }
                else if (valueType == typeof(float))
                    builder.Append("F");
                else if (valueType == typeof(decimal))
                    builder.Append("M");
            }
            else builder.Append("null");
        }
        static void AppendArray<T>(StringBuilder builder, IList<T> list, string start, string end, Action<T> action)
        {
            if (list.Count == 0) return;
            builder.Append(start);
            for (int i = 0; i < list.Count; ++i)
            {
                action(list[i]);
                if (i < list.Count - 1)
                    builder.Append(", ");
            }
            builder.Append(end);
        }
        
        static bool IsMethodOverride(MethodInfo method) => method.GetBaseDefinition() != method;
        static bool IsIndexer(PropertyInfo property) => property.GetIndexParameters().Length > 0;
        static bool IsTuple(Type type) => GetNameWithoutGeneric(type.Name) == "ValueTuple" && type.Namespace == "System" && type.IsGenericType;
        static bool HasAttribute(IEnumerable<CustomAttributeData> attributes, string fullname)
        {
            foreach (var attribute in attributes)
                if (attribute.AttributeType.FullName == fullname)
                return true;
            return false;
        }
    }
#endif
    
    // RANT: This function only gets called in LevelInfoPostProcess.cs and its job is to:
    // 1. Add 2 rule tiles at the 2 ends of a door line.
    // 2. Delete all the tiles that are at the door's tiles.
    // 3. Delete all the tiles that are next to the door's tiles.
    // Originally, I let Edgar's corridor system to handle the first 2 cases (I have to have a shared tilemap).
    // Now, I don't need the corridor system anymore but I still need a shared tilemap.
    // Obviously, these cases can be easily solved if Unity's rule tile system let you work with out-of-bounds cases, but it doesn't :(
    // If Unity ever allow that then I can remove most of this code and also don't need a shared tilemap anymore.
    public void InitTilemap(List<RoomInstance> rooms, Tilemap tilemap, TileBase ruleTile)
    {
        foreach (var room in rooms)
        {
            foreach (var door in room.Doors)
            {
                if (door.ConnectedRoomInstance != null)
                {
                    Vector3Int dir = door.DoorLine.GetDirectionVector();
                    tilemap.SetTile(door.DoorLine.From + room.Position - dir, ruleTile);
                    tilemap.SetTile(door.DoorLine.To + room.Position + dir, ruleTile);
                    
                    foreach (Vector3Int doorTile in door.DoorLine.GetPoints())
                    {
                        Vector3Int pos = doorTile + room.Position;
                        tilemap.SetTile(pos, null);
                        Remove(tilemap, pos, -(Vector3Int)door.FacingDirection);
                        
                        static void Remove(Tilemap tilemap, Vector3Int pos, Vector3Int removeDir)
                        {
                            pos += removeDir;
                            if (tilemap.GetTile(pos))
                            {
                                tilemap.SetTile(pos, null);
                                Remove(tilemap, pos, removeDir);
                            }
                        }
                    }
                }
            }
        }
        
        tilemap.CompressAndRefresh();
        this.rooms = rooms;
        GameManager.tilemap = tilemap;
        Debug.Log(tilemap);
    }
    
    public static Tilemap GetTilemapFromRoom(Transform roomTransform)
    {
        return roomTransform.GetChild(0).GetChild(2).GetComponent<Tilemap>();
    }
    
    public static Bounds GetBoundsFromTilemap(Tilemap tilemap)
    {
        Bounds bounds = tilemap.cellBounds.ToBounds();
        bounds.center += tilemap.transform.position;
        return bounds;
    }
    
    public static Bounds GetBoundsFromRoom(Transform roomTransform)
    {
        return roomTransform != null ? GetBoundsFromTilemap(GetTilemapFromRoom(roomTransform)) : defaultBounds;
    }
    
    public static bool GetGroundPos(Vector2 pos, Vector2 extents, float dirY, out Vector3Int groundPos, out Vector3Int emptyPos)
    {
        RaycastHit2D hitInfo = GameUtils.GroundCheck(pos, extents, dirY, Color.cyan);
        groundPos = Vector3Int.zero;
        emptyPos = Vector3Int.zero;
        if (hitInfo)
        {
            Vector3Int hitPosCeil = QueryTiles(tilemap, hitInfo.point.ToVector2Int(true).ToVector3Int(), true, 0, (int)dirY);
            Vector3Int hitPosFloor = QueryTiles(tilemap, hitInfo.point.ToVector2Int(false).ToVector3Int(), true, 0, (int)dirY);
            groundPos = Mathf.Abs(hitPosFloor.y - hitInfo.point.y) < Mathf.Abs(hitPosCeil.y - hitInfo.point.y) ? hitPosFloor : hitPosCeil;
            emptyPos = groundPos + new Vector3Int(0, -(int)dirY, 0);
        }
        return hitInfo;
    }
    
    public static Rect CalculateMoveRegion(Vector2 pos, Vector2 extents, float dirY)
    {
        Rect result = new Rect();
        Debug.Assert(dirY == Mathf.Sign(dirY), dirY);
        if (GetGroundPos(pos, extents, dirY, out Vector3Int hitPos, out Vector3Int empPos))
        {
            Vector3Int minGroundPos = QueryTiles(tilemap, hitPos, false, -1, 0);
            Vector3Int maxGroundPos = QueryTiles(tilemap, hitPos, false, +1, 0);
            
            Vector3Int minWallPos = QueryTiles(tilemap, empPos, true, -1, 0);
            Vector3Int maxWallPos = QueryTiles(tilemap, empPos, true, +1, 0);
            
            if (dirY < 0)
            {
                minGroundPos.y++;
                maxGroundPos.y++;
            }
            else if (dirY > 0)
            {
                minWallPos.y++;
                maxWallPos.y++;
            }
            
            Vector2 minPos = MathUtils.Max((Vector2Int)minGroundPos, (Vector2Int)minWallPos) + new Vector2(1, 0);
            Vector2 maxPos = MathUtils.Min((Vector2Int)maxGroundPos, (Vector2Int)maxWallPos);
            result = MathUtils.CreateRectMinMax(minPos + extents * new Vector2(+1, -dirY), maxPos + extents * new Vector2(-1, -dirY));
            
            //GameDebug.Log($"Hit pos: {hitPos}, Min pos: {minPos}, Max pos: {maxPos}, Rect: {result}");
            GameDebug.DrawBox(result, Color.green);
            GameDebug.DrawLine((Vector3)minGroundPos, (Vector3)maxGroundPos, Color.red);
            GameDebug.DrawLine((Vector3)minWallPos, (Vector3)maxWallPos, Color.yellow);
        }
        return result;
    }
    
    private static Vector3Int QueryTiles(Tilemap tilemap, Vector3Int startPos, bool breakCondition, int advanceX, int advanceY)
    {
        Vector3Int pos = startPos;
        while (tilemap.GetTile(pos) != breakCondition)
            pos += new Vector3Int(advanceX, advanceY, 0);
        return pos;
    }
    
    private void Start()
    {
        ObjectPooler.Init(gameObject, pools);
        AudioManager.Init(gameObject, audios, firstMusic, sourceCount);
        gameUI = FindObjectOfType<GameUI>();
        mainCam = Camera.main;
        cameraEntity = mainCam.GetComponentInParent<Entity>();
        StartGameMode(startMode);
        
        // Camera
        GameDebug.BindInput(new DebugInput
                            {
                                trigger = InputType.Debug_CameraShake,
                                increase = InputType.Debug_ShakeIncrease,
                                decrease = InputType.Debug_ShakeDecrease,
                                range = new RangedFloat(0, Enum.GetValues(typeof(ShakeMode)).Length - 1),
                                updateValue = (value, dir) => value + dir,
                                callback = mode => { GameDebug.Log((ShakeMode)mode); CameraSystem.instance.Shake((ShakeMode)mode, MathUtils.SmoothStart3); },
                            });
        GameDebug.BindInput(InputType.Debug_CameraShock, () => CameraSystem.instance.Shock(2));
        
        // Log
        GameDebug.BindInput(InputType.Debug_ToggleLog, () => GameDebug.ToggleLogger());
        GameDebug.BindInput(InputType.Debug_ClearLog, () => GameDebug.ClearLog());
        
        // Time
        GameDebug.BindInput(new DebugInput
                            {
                                trigger = InputType.Debug_ResetTime,
                                increase = InputType.Debug_FastTime,
                                decrease = InputType.Debug_SlowTime,
                                value = 1f,
                                range = new RangedFloat(.125f, 4f),
                                updateValue = (value, dir) => value * Mathf.Pow(2, dir),
                                changed = scale => { Time.timeScale = scale; GameDebug.Log(Time.timeScale); },
                                callback = _ => { Time.timeScale = 1; GameDebug.Log("Reset Time.timeScale!"); }
                            });
        
        // VFX
        GameDebug.BindInput(InputType.Debug_SpawnParticle, () => ParticleEffect.instance.SpawnParticle(ParticleType.Explosion, Vector2.zero, 1));
        GameDebug.BindInput(InputType.Debug_TestPlayerVFX, player.TestPlayerVFX);
    }
    
    private void Update()
    {
        GameDebug.UpdateInput();
        
        if (rooms != null)
        {
            if (levels[currentLevel].moveAutomatically)
            {
                if (cameraEntity.CompleteCycle() && currentRoom < rooms.Count)
                {
                    float aspectRatio = 16f / 9f;
                    Transform roomTransform = rooms[currentRoom++].RoomTemplateInstance.transform;
                    Bounds bounds = GetBoundsFromRoom(roomTransform);
                    float ratio = bounds.extents.x / bounds.extents.y;
                    mainCam.orthographicSize = ratio <= aspectRatio ? (bounds.extents.x / aspectRatio) : bounds.extents.y;
                    GameInput.TriggerEvent(GameEventType.NextRoom, roomTransform);
                }
            }
            else
            {
                for (int i = 0; i < rooms.Count; ++i)
                {
                    if (i != currentRoom)
                    {
                        // NOTE: This will make sure the player has already moved pass the doors
                        Rect roomRect = GetBoundsFromRoom(rooms[i].RoomTemplateInstance.transform).ToRect().Resize(Vector2.one * 2, Vector2.one * -2);
                        if (roomRect.Contains(player.transform.position))
                        {
                            GameInput.TriggerEvent(GameEventType.NextRoom, rooms[i].RoomTemplateInstance.transform);
                            currentRoom = i;
                            break;
                        }
                    }
                }
            }
        }
    }
    
    public void StartGameMode(GameMode mode)
    {
        if (mode == GameMode.None || mode == GameMode.Count)
            return;
        if (mode == GameMode.Quit)
        {
            Application.Quit();
            return;
        }
        
        // Reset the game
        {
            // TODO:
            //  - Reset all the scriptable objects
            //  - Reset all game input's events
            //  - Reset persistent entities like camera
            rooms?.Clear();
            gameMenu.gameObject.SetActive(false);
            
            bool isMainMode = mode == GameMode.Main;
            // NOTE: I didn't use the ?. operator because Unity's GameObject has a custom == operator but doesn't have a custom ?. operator
            if (mainMode) mainMode.SetActive(isMainMode);
            playMode.SetActive(!isMainMode);
        }
        
        switch (mode)
        {
            case GameMode.Main:
            {
                AudioManager.PlayAudio(AudioType.Music_Main);
            }
            break;
            case GameMode.Play:
            {
                {
                    for (int i = 0; i < levels.Length; i++)
                        levels[i].gameObject.SetActive(i == currentLevel);
                    if (overrideGameUI)
                    {
                        gameUI.displayHealthBar = gameUI.displayMoneyCount = gameUI.displayWaveCount = gameUI.displayWeaponUI = true;
                        gameUI.enabled = levels[currentLevel].enableUI;
                        gameUI.displayMinimap = levels[currentLevel].enableMinimap;
                    }
                }
                
                LevelData level = levels[currentLevel];
                cameraEntity.properties.SetProperty(EntityProperty.CustomInit, true);
                cameraEntity.CustomInit();
                cameraEntity.InitCamera(level.moveAutomatically, level.useSmoothDamp, level.cameraValue, level.waitTime);
                
                switch (level.type)
                {
                    case BoundsType.Generator:
                    {
                        {
                            bool levelGenerated = false;
                            int count = level.maxGenerateTry;
                            DungeonGenerator generator = level.generator;
                            generator.transform.Clear();
                            
                            while (!levelGenerated)
                            {
                                try
                                {
                                    if (count == 0)
                                    {
                                        Debug.LogError("Level couldn't be generated!");
                                        break;
                                    }
                                    generator.Generate();
                                    levelGenerated = true;
                                }
                                catch (InvalidOperationException)
                                {
                                    Debug.LogError("Timeout encountered");
                                    count--;
                                }
                            }
                        }
                        
                        if (!level.moveAutomatically)
                        {
                            currentRoom = -1;
                            GameInput.BindEvent(GameEventType.EndRoom, room => LockRoom(room, false, false));
                            GameInput.BindEvent(GameEventType.NextRoom, room => LockRoom(room, true, level.disableEnemies));
                        }
                        else
                        {
                            currentRoom = 0;
                            PlayerController player = FindObjectOfType<PlayerController>();
                            if (player)
                                Destroy(player.gameObject);
                        }
                    }
                    break;
                    case BoundsType.Tilemap:
                    {
                        tilemap = level.tilemap.CompressAndRefresh();
                        defaultBounds = level.tilemap.cellBounds.ToBounds();
                        GameInput.TriggerEvent(GameEventType.NextRoom, null);
                    }
                    break;
                    case BoundsType.Custom:
                    {
                        defaultBounds = new Bounds(Vector3.zero, level.boundsSize + mainCam.HalfSize() * 2);
                        GameInput.TriggerEvent(GameEventType.NextRoom, null);
                    }
                    break;
                }
            }
            break;
        }
        
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Entity>();
    }
    
    static void LockRoom(Transform room, bool lockRoom, bool disableEnemies)
    {
        if (lockRoom && !disableEnemies)
        {
            EnemySpawner spawner = room?.GetComponentInChildren<EnemySpawner>(true);
            if (!spawner)
                return;
            spawner.enabled = true;
        }
        
        Transform doorHolder = room?.Find("Doors");
        if (doorHolder)
        {
            if (lockRoom)
                doorHolder.gameObject.SetActive(true);
            else
                Destroy(doorHolder.gameObject, 1f);
            
            foreach (Transform door in doorHolder)
            {
                Animator doorAnim = door.GetComponent<Animator>();
                doorAnim.Play(lockRoom ? "Lock" : "Unlock");
            }
        }
    }
    
    //private void OnAudioFilterRead(float[] data, int channels) => AudioManager.ReadAudio(data, channels);
}
