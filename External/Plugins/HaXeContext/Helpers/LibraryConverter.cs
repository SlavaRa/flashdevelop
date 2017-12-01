using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using AS3Context;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore.Managers;

namespace HaXeContext.Helpers
{
    class LibraryConverter
    {
        /// <summary>
        /// Create virtual FileModel objects from Abc bytecode
        /// </summary>
        /// <param name="path"></param>
        /// <param name="context"></param>
        public static void Convert(PathModel path, IASContext context)
        {
            var ext = Path.GetExtension(path.Path).ToLower();
            if (ext == ".dll")
            {
                var models = new Dictionary<string, FileModel>();
                var assembly = Assembly.LoadFile(path.Path);
                var modules = assembly.GetModules(false);
                foreach (var module in modules)
                {
                    var types = module.GetTypes();
                    foreach (var typeInfo in types)
                    {
                        var fileName = AbcConverter.reSafeChars.Replace(typeInfo.Name, "_").TrimEnd('$');
                        var model = new FileModel(Path.Combine(path.Path, fileName));
                        model.Context = context;
                        model.Classes = new List<ClassModel>();
                        var type = new ClassModel();
                        type.InFile = model;
                        type.Type = GetType(typeInfo);
                        type.Name = typeInfo.Name;
                        type.Flags = FlagType.Class;
                        if (typeInfo.IsInterface) type.Flags |= FlagType.Interface;
                        else if (typeInfo.IsEnum) type.Flags |= FlagType.Enum;
                        else
                        {
                            if (typeInfo.IsSealed) type.Flags |= FlagType.Final;
                        }
                        var interfaces = typeInfo.GetInterfaces();
                        if (interfaces.Length > 0)
                        {
                            type.Implements = new List<string>(interfaces.Length);
                            foreach (var it in interfaces)
                            {
                                type.Implements.Add(GetType(it));
                            }
                        }
                        type.Members = new MemberList();
                        if (typeInfo.IsClass)
                        {
                            foreach (var it in typeInfo.GetConstructors())
                            {
                                var member = new MemberModel();
                                member.Name = it.Name;
                                member.Flags = FlagType.Constructor;
                                var declaringType = it.DeclaringType;
                                if (declaringType != null)
                                {
                                    member.Type = GetType(declaringType);
                                    if (declaringType.IsPublic) member.Access = Visibility.Public;
                                }
                                type.Members.Add(member);
                            }
                            foreach (var it in typeInfo.GetProperties())
                            {
                                var member = new MemberModel();
                                member.Name = it.Name;
                                member.Flags = FlagType.Variable;
                                var declaringType = it.DeclaringType;
                                if (declaringType != null)
                                {
                                    member.Type = GetType(declaringType);
                                    if (declaringType.IsPublic) member.Access = Visibility.Public;
                                }
                                type.Members.Add(member);
                            }
                            foreach (var it in typeInfo.GetFields())
                            {
                                var member = new MemberModel();
                                member.Name = it.Name;
                                member.Flags = FlagType.Variable;
                                var declaringType = it.DeclaringType;
                                if (declaringType != null)
                                {
                                    member.Type = GetType(declaringType);
                                    if (declaringType.IsPublic) member.Access = Visibility.Public;
                                }
                                type.Members.Add(member);
                            }
                            foreach (var it in typeInfo.GetMethods())
                            {
                                var member = new MemberModel();
                                member.Name = it.Name;
                                member.Flags = FlagType.Function;
                                var declaringType = it.DeclaringType;
                                if (declaringType != null)
                                {
                                    member.Type = GetType(declaringType);
                                    if (declaringType.IsPublic) member.Access = Visibility.Public;
                                    var parameterInfos = it.GetParameters();
                                    member.Parameters = new List<MemberModel>(parameterInfos.Length);
                                    foreach (var parameterInfo in parameterInfos)
                                    {
                                        var param = new MemberModel();
                                        param.Flags = FlagType.ParameterVar | FlagType.Variable;
                                        param.Name = parameterInfo.Name;
                                        param.Type = GetType(parameterInfo.ParameterType);
                                        member.Parameters.Add(param);
                                    }
                                }
                                type.Members.Add(member);
                            }
                        }
                        if ((type.Flags & FlagType.Interface) != 0)
                        {
                            if (interfaces.Length > 0)
                            {
                                type.ExtendsType = type.Implements[0];
                                type.Implements.RemoveAt(0);
                                if (type.Implements.Count == 0) type.Implements = null;
                            }
                            foreach (MemberModel it in type.Members)
                            {
                                it.Access = Visibility.Public;
                            }
                        }
                        else type.ExtendsType = typeInfo.BaseType?.FullName;
                        model.Classes.Add(type);
                        models[model.FileName] = model;
                    }
                }
                path.SetFiles(models);
            }
        }

        static string GetType(Type type)
        {
            if (type.IsGenericType)
            {
                return type.Name;
            }
            return type.FullName;
        }
    }
}
