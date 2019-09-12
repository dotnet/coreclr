// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ILCompiler.IBC;

using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;
using System.Text;

namespace ILCompiler
{
    internal static class ValueTextWorkaround
    {
        public static bool ValueTextEquals(this Utf8JsonReader json, string strToCompare)
        {
            return json.GetString() == strToCompare;
        }
    }


    [Flags]
    public enum MethodProfilingDataFlags
    {
        // Important: update toolbox\ibcmerge\ibcmerge.cs if you change these
        ReadMethodCode = 0,  // 0x00001  // Also means the method was executed
        ReadMethodDesc = 1,  // 0x00002
        RunOnceMethod = 2,  // 0x00004
        RunNeverMethod = 3,  // 0x00008
                             //  MethodStoredDataAccess        = 4,  // 0x00010  // obsolete
        WriteMethodDesc = 5,  // 0x00020
                              //  ReadFCallHash                 = 6,  // 0x00040  // obsolete
        ReadGCInfo = 7,  // 0x00080
        CommonReadGCInfo = 8,  // 0x00100
                               //  ReadMethodDefRidMap           = 9,  // 0x00200  // obsolete
        ReadCerMethodList = 10, // 0x00400
        ReadMethodPrecode = 11, // 0x00800
        WriteMethodPrecode = 12, // 0x01000
        ExcludeHotMethodCode = 13, // 0x02000  // Hot method should be excluded from the ReadyToRun image
        ExcludeColdMethodCode = 14, // 0x04000  // Cold method should be excluded from the ReadyToRun image
        DisableInlining = 15, // 0x08000  // Disable inlining of this method in optimized AOT native code
    }

    public class MethodProfileData
    {
        public MethodProfileData(MethodDesc method, MethodProfilingDataFlags flags, uint scenarioMask)
        {
            Method = method;
            Flags = flags;
            ScenarioMask = scenarioMask;
        }
        public readonly MethodDesc Method;
        public readonly MethodProfilingDataFlags Flags;
        public readonly uint ScenarioMask;
    }

    public abstract class ProfileData
    {
        public abstract bool PartialNGen { get; }
        public abstract MethodProfileData GetMethodProfileData(MethodDesc m);
        public abstract IEnumerable<MethodProfileData> GetAllMethodProfileData();
        public abstract byte[] GetMethodBlockCount(MethodDesc m);

        public static void SerializeToJSon(ProfileData data, Stream stream)
        {
            CustomAttributeTypeNameFormatter customAttributeTypeNameFormatter = new CustomAttributeTypeNameFormatter();
            JsonWriterOptions options = new JsonWriterOptions();
            options.Indented = true;
            using (Utf8JsonWriter json = new Utf8JsonWriter(stream, options))
            {
                json.WriteStartObject();
                json.WriteBoolean("PartialNGen", data.PartialNGen);
                json.WriteStartArray("Methods");
                JsonEncodedText jsonType = JsonEncodedText.Encode("Type");
                JsonEncodedText jsonMethod = JsonEncodedText.Encode("Method");
                JsonEncodedText jsonSig = JsonEncodedText.Encode("Sig");
                JsonEncodedText jsonInst = JsonEncodedText.Encode("Inst");
                JsonEncodedText jsonFlags = JsonEncodedText.Encode("Flags");
                JsonEncodedText jsonInstCount = JsonEncodedText.Encode("InstCount");
                foreach (MethodProfileData methodData in data.GetAllMethodProfileData())
                {
                    string methodType = null;
                    string methodName = null;
                    string methodSignature = null;
                    List<string> methodInstantiationArguments = null;
                    int? instCount = null;

                    try
                    {
                        methodType = customAttributeTypeNameFormatter.FormatName(methodData.Method.OwningType, true);
                        MethodDesc uninstantiatedMethod = methodData.Method.GetMethodDefinition();
                        methodName = uninstantiatedMethod.Name;
                        methodSignature = uninstantiatedMethod.Signature.ToString(includeReturnType: true);
                        methodInstantiationArguments = null;
                        if (methodData.Method.HasInstantiation)
                        {
                            if (methodData.Method.IsGenericMethodDefinition)
                            {
                                instCount = methodData.Method.Instantiation.Length;
                            }
                            else
                            {
                                methodInstantiationArguments = new List<string>();
                                foreach (TypeDesc type in methodData.Method.Instantiation)
                                {
                                    methodInstantiationArguments.Add(customAttributeTypeNameFormatter.FormatName(type, true));
                                }
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    json.WriteStartObject();
                    json.WriteString(jsonType, methodType);
                    json.WriteString(jsonMethod, methodName);
                    json.WriteString(jsonSig, methodSignature);
                    if (methodInstantiationArguments != null)
                    {
                        json.WriteStartArray(jsonInst);
                        foreach (string s in methodInstantiationArguments)
                        {
                            json.WriteStringValue(s);
                        }
                        json.WriteEndArray();
                    }
                    if (instCount != null)
                    {
                        json.WriteNumber(jsonInstCount, instCount.Value);
                    }
                    json.WriteNumber(jsonFlags, (int)methodData.Flags);
                    json.WriteEndObject();
                }
                json.WriteEndArray();
                json.WriteEndObject();
            }
        }

        private enum ProfileDataParseState
        {
            Start,
            OuterObject,
            ParsingPartialNGen,
            ParsingMethodsProperty,
            ParsingMethodsArray,
            ParsingMethod,
            ParsingMethodType,
            ParsingMethodName,
            ParsingMethodSignature,
            ParsingMethodInstantiationProperty,
            ParsingMethodInstantiation,
            ParsingMethodFlags,
            ParsingMethodInstCount,
            Done,
        }

        private static TypeDesc LoadTypeFromString(TypeSystemContext context, Logger logger, string typeName)
        {
            ModuleDesc systemModule = context.SystemModule;

            TypeDesc foundType = systemModule.GetTypeByCustomAttributeTypeName(typeName, false, (typeDefName, module, throwIfNotFound) =>
            {
                return (MetadataType)context.GetCanonType(typeDefName)
                    ?? CustomAttributeTypeNameParser.ResolveCustomAttributeTypeDefinitionName(typeDefName, module, throwIfNotFound);
            });
            if (foundType == null)
            {
                logger.Writer.WriteLine($"Unable to load '{typeName}'");
                return null;
            }

            return foundType;
        }

        private static MethodDesc LoadMethodFromStrings(TypeSystemContext context, Logger logger, string methodType, string methodName, string methodSignature, List<string> methodInstantiationArguments, int? instCount)
        {
            TypeDesc type = LoadTypeFromString(context, logger, methodType);
            if (type == null)
                return null;

            MethodDesc uninstantiatedMethod = null;
            foreach (MethodDesc m in type.GetMethods())
            {
                if (m.Name == methodName)
                {
                    try
                    {
                        if (m.Signature.ToString(includeReturnType: true) == methodSignature)
                        {
                            if ((methodInstantiationArguments != null) && (methodInstantiationArguments.Count != m.Instantiation.Length))
                            {
                                // Not matching number of generic arguments on method for instantiated method
                            }
                            else if (instCount.HasValue && m.Instantiation.Length != instCount.Value)
                            {
                                // Not matching number of generic arguments on method definition
                            }
                            else
                            {
                                uninstantiatedMethod = m;
                                break;
                            }
                        }
                    }
                    catch
                    { }
                }
            }

            if (uninstantiatedMethod != null)
            {
                if (methodInstantiationArguments != null) 
                {
                    if (methodInstantiationArguments.Count == uninstantiatedMethod.Instantiation.Length)
                    {
                        TypeDesc[] instArgs = new TypeDesc[methodInstantiationArguments.Count];
                        for (int i = 0; i < instArgs.Length; i++)
                        {
                            instArgs[i] = LoadTypeFromString(context, logger, methodInstantiationArguments[i]);
                        }
                        return uninstantiatedMethod.MakeInstantiatedMethod(new Instantiation(instArgs));
                    }
                }
                else
                {
                    return uninstantiatedMethod;
                }
            }
            else
            {
                logger.Writer.WriteLine($"Unable to load '{methodType}'.'{methodName}' '{methodSignature}");
            }

            return null;
        }

        public static ProfileData ReadJsonData(Logger logger, byte[] jsonData, TypeSystemContext context)
        {
            List<MethodProfileData> data = new List<MethodProfileData>();
            JsonReaderOptions options = new JsonReaderOptions();
            options.AllowTrailingCommas = true;
            options.CommentHandling = JsonCommentHandling.Skip;
            JsonReaderState readerState = new JsonReaderState();
            Utf8JsonReader json = new Utf8JsonReader(jsonData, true, readerState);
            bool? partialNGen = null;
            ProfileDataParseState state = ProfileDataParseState.Start;

            string methodType = null;
            string methodName = null;
            string methodSignature = null;
            List<string> methodInstantiationArguments = null;
            MethodProfilingDataFlags? flags = (MethodProfilingDataFlags)0;
            int? methodInstCount = null;

            while (json.Read())
            {
                JsonTokenType tokenType = json.TokenType;
                switch (tokenType)
                {
                    case JsonTokenType.StartObject:
                        switch (state)
                        {
                            case ProfileDataParseState.Start:
                                state = ProfileDataParseState.OuterObject;
                                break;

                            case ProfileDataParseState.ParsingMethodsArray:
                                state = ProfileDataParseState.ParsingMethod;
                                methodType = null;
                                methodName = null;
                                methodSignature = null;
                                methodInstantiationArguments = null;
                                flags = null;
                                methodInstCount = null;
                                break;

                            default:
                                throw new Exception("Ibc parse error");
                        }
                        break;

                    case JsonTokenType.EndObject:
                        switch (state)
                        {
                            case ProfileDataParseState.OuterObject:
                                state = ProfileDataParseState.Done;
                                break;

                            case ProfileDataParseState.ParsingMethod:
                                if (methodType == null) throw new Exception("Ibc parse error: Type missing");
                                if (methodName == null) throw new Exception("Ibc parse error: Method missing");
                                if (methodSignature == null) throw new Exception("Ibc parse error: Sig missing");
                                if (!flags.HasValue) throw new Exception("Ibc parse error: Flags missing");

                                MethodDesc m = LoadMethodFromStrings(context, logger, methodType, methodName, methodSignature, methodInstantiationArguments, methodInstCount);

                                state = ProfileDataParseState.ParsingMethodsArray;
                                if (m != null)
                                {
                                    MethodProfileData profData = new MethodProfileData(m, flags.Value, 0xFFFFFFFF);
                                    data.Add(profData);
                                }
                                break;

                            default:
                                throw new Exception("Ibc parse error");
                        }
                        break;

                    case JsonTokenType.PropertyName:
                        {
                            bool unknownProperty = false;
                            switch (state)
                            {
                                case ProfileDataParseState.OuterObject:
                                    if (json.ValueTextEquals("PartialNGen"))
                                        state = ProfileDataParseState.ParsingPartialNGen;
                                    else if (json.ValueTextEquals("Methods"))
                                        state = ProfileDataParseState.ParsingMethodsProperty;
                                    else
                                        unknownProperty = true;
                                    break;
                                case ProfileDataParseState.ParsingMethod:
                                    if (json.ValueTextEquals("Type"))
                                        state = ProfileDataParseState.ParsingMethodType;
                                    else if (json.ValueTextEquals("Method"))
                                        state = ProfileDataParseState.ParsingMethodName;
                                    else if (json.ValueTextEquals("Sig"))
                                        state = ProfileDataParseState.ParsingMethodSignature;
                                    else if (json.ValueTextEquals("Flags"))
                                        state = ProfileDataParseState.ParsingMethodFlags;
                                    else if (json.ValueTextEquals("InstCount"))
                                        state = ProfileDataParseState.ParsingMethodInstCount;
                                    else if (json.ValueTextEquals("Inst"))
                                    {
                                        state = ProfileDataParseState.ParsingMethodInstantiationProperty;
                                        methodInstantiationArguments = new List<string>();
                                    }
                                    else
                                        unknownProperty = true;
                                    break;
                                default:
                                    throw new Exception("Ibc parse error");
                            }
                            if (unknownProperty)
                            {
                                // Unknown property value, skip
                                if (!json.TrySkip())
                                {
                                    throw new Exception($"Unable to parse {json.GetString()}");
                                }
                            }
                        }
                        break;

                    case JsonTokenType.StartArray:
                        {
                            switch (state)
                            {
                                case ProfileDataParseState.ParsingMethodsProperty:
                                    state = ProfileDataParseState.ParsingMethodsArray;
                                    break;
                                case ProfileDataParseState.ParsingMethodInstantiationProperty:
                                    state = ProfileDataParseState.ParsingMethodInstantiation;
                                    break;
                                default:
                                    throw new Exception("Ibc parse error");
                            }
                        }

                        break;

                    case JsonTokenType.EndArray:
                        switch (state)
                        {
                            case ProfileDataParseState.ParsingMethodsArray:
                                state = ProfileDataParseState.OuterObject;
                                break;
                            case ProfileDataParseState.ParsingMethodInstantiation:
                                state = ProfileDataParseState.ParsingMethod;
                                break;
                            default:
                                throw new Exception("Ibc parse error");
                        }
                        break;

                    case JsonTokenType.False:
                    case JsonTokenType.True:
                        switch (state)
                        {
                            case ProfileDataParseState.ParsingPartialNGen:
                                partialNGen = json.GetBoolean();
                                state = ProfileDataParseState.OuterObject;
                                break;
                            default:
                                throw new Exception("Ibc parse error");
                        }
                        break;

                    case JsonTokenType.String:
                        string parsedString = json.GetString();

                        switch (state)
                        {
                            case ProfileDataParseState.ParsingMethodName:
                                methodName = parsedString; state = ProfileDataParseState.ParsingMethod; break;
                            case ProfileDataParseState.ParsingMethodType:
                                methodType = parsedString; state = ProfileDataParseState.ParsingMethod; break;
                            case ProfileDataParseState.ParsingMethodSignature:
                                methodSignature = parsedString; state = ProfileDataParseState.ParsingMethod; break;
                            case ProfileDataParseState.ParsingMethodInstantiation:
                                methodInstantiationArguments.Add(parsedString); break;
                            default:
                                throw new Exception("Ibc parse error");
                        }
                        break;

                    case JsonTokenType.Number:
                        switch (state)
                        {
                            case ProfileDataParseState.ParsingMethodFlags:
                                flags = (MethodProfilingDataFlags)json.GetInt32();
                                state = ProfileDataParseState.ParsingMethod;
                                break;
                            case ProfileDataParseState.ParsingMethodInstCount:
                                methodInstCount = json.GetInt32();
                                state = ProfileDataParseState.ParsingMethod;
                                break;
                            default:
                                throw new Exception("Ibc parse error");
                        }
                        break;
                }
            }

            if (state != ProfileDataParseState.Done)
                throw new Exception("Ibc parse error, didn't find end");

            IBCProfileData profileData = new IBCProfileData(partialNGen.HasValue ? partialNGen.Value : false, data);
            return profileData;
        }
    }

    public class EmptyProfileData : ProfileData
    {
        private static readonly EmptyProfileData s_singleton = new EmptyProfileData();

        private EmptyProfileData()
        {
        }

        public override bool PartialNGen => false;

        public static EmptyProfileData Singleton => s_singleton;

        public override MethodProfileData GetMethodProfileData(MethodDesc m)
        {
            return null;
        }

        public override IEnumerable<MethodProfileData> GetAllMethodProfileData()
        {
            return Array.Empty<MethodProfileData>();
        }

        public override byte[] GetMethodBlockCount(MethodDesc m)
        {
            return null;
        }
    }


    public class ProfileDataManager
    {
        private readonly IBCProfileParser _ibcParser;
        private readonly List<ModuleDesc> _inputModules;
        private readonly List<ProfileData> _inputTibc = new List<ProfileData>();
        private readonly bool _nonLocalizedGenerics;

        public ProfileDataManager(Logger logger, IEnumerable<ModuleDesc> possibleReferenceModules, IEnumerable<ModuleDesc> inputModules, IReadOnlyList<string> tibcFiles, TypeSystemContext context, bool nonLocalizedGenerics)
        {
            _ibcParser = new IBCProfileParser(logger, possibleReferenceModules);
            _inputModules = new List<ModuleDesc>(inputModules);
            _nonLocalizedGenerics = nonLocalizedGenerics;
            foreach (string file in tibcFiles)
            {
                byte[] tibcData = File.ReadAllBytes(file);
                _inputTibc.Add(ProfileData.ReadJsonData(logger, tibcData, context));
            }
        }

        private readonly Dictionary<ModuleDesc, ProfileData> _profileData = new Dictionary<ModuleDesc, ProfileData>();

        public ProfileData GetDataForModuleDesc(ModuleDesc moduleDesc)
        {
            lock (_profileData)
            {
                if (_profileData.TryGetValue(moduleDesc, out ProfileData precomputedProfileData))
                    return precomputedProfileData;
            }

            ProfileData computedProfileData = ComputeDataForModuleDesc(moduleDesc);

            lock (_profileData)
            {
                if (_profileData.TryGetValue(moduleDesc, out ProfileData precomputedProfileData))
                    return precomputedProfileData;

                _profileData.Add(moduleDesc, computedProfileData);
                return computedProfileData;
            }
        }

        private ProfileData ComputeDataForModuleDesc(ModuleDesc moduleDesc)
        {
            if (!(moduleDesc is EcmaModule ecmaModule))
                return EmptyProfileData.Singleton;

            ProfileData embeddedIbcProfileData = _ibcParser.ParseIBCDataFromModule(ecmaModule);
            if (embeddedIbcProfileData == null)
                embeddedIbcProfileData = EmptyProfileData.Singleton;

            // Merge Ibc data from embedded data and other sources
            bool partialNgen = false;
            Dictionary<MethodDesc, MethodProfileData> mergedProfileData = new Dictionary<MethodDesc, MethodProfileData>();

            MergeProfileData(ref partialNgen, mergedProfileData, embeddedIbcProfileData);
            foreach (ProfileData profileData in _inputTibc)
            {
                MergeProfileData(ref partialNgen, mergedProfileData, profileData);
            }

            return new IBCProfileData(partialNgen, mergedProfileData.Values);
        }

        private void MergeProfileData(ref bool partialNgen, Dictionary<MethodDesc, MethodProfileData> mergedProfileData, ProfileData profileData)
        {
            if (profileData.PartialNGen)
                partialNgen = true;

            foreach (MethodProfileData data in profileData.GetAllMethodProfileData())
            {
                MethodProfileData dataToMerge;
                if (mergedProfileData.TryGetValue(data.Method, out dataToMerge))
                {
                    mergedProfileData[data.Method] = new MethodProfileData(data.Method, dataToMerge.Flags | data.Flags, dataToMerge.ScenarioMask | data.ScenarioMask);
                }
                else if (ShouldDataBeMergedIn(data.Method))
                {
                    mergedProfileData.Add(data.Method, data);
                }
            }
        }

        private bool InstantiationTypeShouldBeInstantiatedInSpecificModule(TypeDesc t, ModuleDesc module)
        {
            if (t == t.Context.CanonType)
                return true;

            if (t.IsPrimitive)
                return true;

            if (t is MetadataType mdType)
            {
                if (mdType.Module != module)
                    return false;
            }

            foreach (TypeDesc type in t.Instantiation)
            {
                if (!InstantiationTypeShouldBeInstantiatedInSpecificModule(type, module))
                    return false;
            }

            return true;
        }

        private bool ShouldDataBeMergedIn(MethodDesc m)
        {
            ModuleDesc singleAssemblyModule = null;

            if (m.OwningType is MetadataType metadataType)
                singleAssemblyModule = metadataType.Module;
            else
                return false;

            foreach (TypeDesc instType in m.Instantiation)
            {
                if (!InstantiationTypeShouldBeInstantiatedInSpecificModule(instType, singleAssemblyModule))
                {
                    singleAssemblyModule = null;
                    break;
                }
            }

            if (singleAssemblyModule != null)
            {
                foreach (TypeDesc instType in m.OwningType.Instantiation)
                {
                    if (!InstantiationTypeShouldBeInstantiatedInSpecificModule(instType, singleAssemblyModule))
                    {
                        singleAssemblyModule = null;
                        break;
                    }
                }
            }

            bool methodMatchesSingleAssembly = false;

            if (singleAssemblyModule != null)
            {
                foreach (ModuleDesc module in _inputModules)
                {
                    if (singleAssemblyModule == module)
                    {
                        methodMatchesSingleAssembly = true;
                        break;
                    }
                }

                return methodMatchesSingleAssembly;
            }
            else if (_nonLocalizedGenerics)
            {
                return true;
            }

            return false;
        }
    }
}
