#!  /usr/bin/env python

import argparse
import glob
import json
import hashlib
import os
import tempfile
import sys
import subprocess

g_Framework_Assemblies = [
    'CommandLine.dll',
    'Microsoft.CodeAnalysis.CSharp.dll',
    'Microsoft.CodeAnalysis.dll',
    'Microsoft.CodeAnalysis.VisualBasic.dll',
    'Microsoft.CSharp.dll',
    'Microsoft.Diagnostics.FastSerialization.dll',
    'Microsoft.Diagnostics.Tracing.TraceEvent.dll',
    'Microsoft.DotNet.Cli.Utils.dll',
    'Microsoft.DotNet.InternalAbstractions.dll',
    'Microsoft.DotNet.ProjectModel.dll',
    'Microsoft.Extensions.DependencyModel.dll',
#   'Microsoft.VisualBasic.dll', see https://github.com/dotnet/coreclr/issues/19370
    'Microsoft.Win32.Primitives.dll',
    'Microsoft.Win32.Registry.dll',
    'netstandard.dll',
    'Newtonsoft.Json.dll',
    'NuGet.Common.dll'
    'NuGet.Configuration.dll',
    'NuGet.DependencyResolver.Core.dll',
    'NuGet.Frameworks.dll',
    'NuGet.LibraryModel.dll',
    'NuGet.Packaging.Core.dll',
    'NuGet.Packaging.Core.Types.dll',
    'NuGet.Packaging.dll',
    'NuGet.ProjectModel.dll',
    'NuGet.Protocol.Core.Types.dll',
    'NuGet.Protocol.Core.v3.dll',
    'NuGet.Repositories.dll',
    'NuGet.RuntimeModel.dll',
    'NuGet.Versioning.dll',
    'System.AppContext.dll',
    'System.Buffers.dll',
    'System.Collections.Concurrent.dll',
    'System.Collections.dll',
    'System.Collections.Immutable.dll',
    'System.Collections.NonGeneric.dll',
    'System.Collections.Specialized.dll',
    'System.CommandLine.dll',
    'System.ComponentModel.Annotations.dll',
    'System.ComponentModel.DataAnnotations.dll',
    'System.ComponentModel.dll',
    'System.ComponentModel.EventBasedAsync.dll',
    'System.ComponentModel.Primitives.dll',
    'System.ComponentModel.TypeConverter.dll',
    'System.Configuration.dll',
    'System.Console.dll',
    'System.Core.dll',
    'System.Data.Common.dll',
    'System.Data.dll',
    'System.Diagnostics.Contracts.dll',
    'System.Diagnostics.Debug.dll',
    'System.Diagnostics.DiagnosticSource.dll',
    'System.Diagnostics.EventLog.dll',
    'System.Diagnostics.FileVersionInfo.dll',
    'System.Diagnostics.Process.dll',
    'System.Diagnostics.StackTrace.dll',
    'System.Diagnostics.TextWriterTraceListener.dll',
    'System.Diagnostics.Tools.dll',
    'System.Diagnostics.TraceSource.dll',
    'System.Diagnostics.Tracing.dll',
    'System.dll',
    'System.Drawing.dll',
    'System.Drawing.Primitives.dll',
    'System.Dynamic.Runtime.dll',
    'System.Globalization.Calendars.dll',
    'System.Globalization.dll',
    'System.Globalization.Extensions.dll',
    'System.IO.Compression.Brotli.dll',
    'System.IO.Compression.dll',
    'System.IO.Compression.FileSystem.dll',
    'System.IO.Compression.ZipFile.dll',
    'System.IO.dll',
    'System.IO.FileSystem.AccessControl.dll',
    'System.IO.FileSystem.dll',
    'System.IO.FileSystem.DriveInfo.dll',
    'System.IO.FileSystem.Primitives.dll',
    'System.IO.FileSystem.Watcher.dll',
    'System.IO.IsolatedStorage.dll',
    'System.IO.MemoryMappedFiles.dll',
    'System.IO.Pipes.AccessControl.dll',
    'System.IO.Pipes.dll',
    'System.IO.UnmanagedMemoryStream.dll',
    'System.Linq.dll',
    'System.Linq.Expressions.dll',
    'System.Linq.Parallel.dll',
    'System.Linq.Queryable.dll',
    'System.Memory.dll',
    'System.Net.dll',
    'System.Net.Http.dll',
    'System.Net.HttpListener.dll',
    'System.Net.Mail.dll',
    'System.Net.NameResolution.dll',
    'System.Net.NetworkInformation.dll',
    'System.Net.Ping.dll',
    'System.Net.Primitives.dll',
    'System.Net.Requests.dll',
    'System.Net.Security.dll',
    'System.Net.ServicePoint.dll',
    'System.Net.Sockets.dll',
    'System.Net.WebClient.dll',
    'System.Net.WebHeaderCollection.dll',
    'System.Net.WebProxy.dll',
    'System.Net.WebSockets.Client.dll',
    'System.Net.WebSockets.dll',
    'System.Numerics.dll',
    'System.Numerics.Vectors.dll',
    'System.ObjectModel.dll',
    'System.Private.DataContractSerialization.dll',
    'System.Private.Uri.dll',
    'System.Private.Xml.dll',
    'System.Private.Xml.Linq.dll',
    'System.Reflection.DispatchProxy.dll',
    'System.Reflection.dll',
    'System.Reflection.Emit.dll',
    'System.Reflection.Emit.ILGeneration.dll',
    'System.Reflection.Emit.Lightweight.dll',
    'System.Reflection.Extensions.dll',
    'System.Reflection.Metadata.dll',
    'System.Reflection.Primitives.dll',
    'System.Reflection.TypeExtensions.dll',
    'System.Resources.Reader.dll',
    'System.Resources.ResourceManager.dll',
    'System.Resources.Writer.dll',
    'System.Runtime.CompilerServices.Unsafe.dll',
    'System.Runtime.CompilerServices.VisualC.dll',
    'System.Runtime.dll',
    'System.Runtime.Extensions.dll',
    'System.Runtime.Handles.dll',
    'System.Runtime.InteropServices.dll',
    'System.Runtime.InteropServices.RuntimeInformation.dll',
    'System.Runtime.InteropServices.WindowsRuntime.dll',
    'System.Runtime.Intrinsics.Experimental.dll',
    'System.Runtime.Loader.dll',
    'System.Runtime.Numerics.dll',
    'System.Runtime.Serialization.dll',
    'System.Runtime.Serialization.Formatters.dll',
    'System.Runtime.Serialization.Json.dll',
    'System.Runtime.Serialization.Primitives.dll',
    'System.Runtime.Serialization.Xml.dll',
    'System.Security.AccessControl.dll',
    'System.Security.Claims.dll',
    'System.Security.Cryptography.Algorithms.dll',
    'System.Security.Cryptography.Cng.dll',
    'System.Security.Cryptography.Csp.dll',
    'System.Security.Cryptography.Encoding.dll',
    'System.Security.Cryptography.OpenSsl.dll',
    'System.Security.Cryptography.Primitives.dll',
    'System.Security.Cryptography.X509Certificates.dll',
    'System.Security.dll',
    'System.Security.Permissions.dll',
    'System.Security.Principal.dll',
    'System.Security.Principal.Windows.dll',
    'System.Security.SecureString.dll',
    'System.ServiceModel.Web.dll',
    'System.ServiceProcess.dll',
    'System.Text.Encoding.CodePages.dll',
    'System.Text.Encoding.dll',
    'System.Text.Encoding.Extensions.dll',
    'System.Text.RegularExpressions.dll',
    'System.Threading.AccessControl.dll',
    'System.Threading.dll',
    'System.Threading.Overlapped.dll',
    'System.Threading.Tasks.Dataflow.dll',
    'System.Threading.Tasks.dll',
    'System.Threading.Tasks.Extensions.dll',
    'System.Threading.Tasks.Parallel.dll',
    'System.Threading.Thread.dll',
    'System.Threading.ThreadPool.dll',
    'System.Threading.Timer.dll',
    'System.Transactions.dll',
    'System.Transactions.Local.dll',
    'System.ValueTuple.dll',
    'System.Web.dll',
    'System.Web.HttpUtility.dll',
    'System.Windows.dll',
    'System.Xml.dll',
    'System.Xml.Linq.dll',
    'System.Xml.ReaderWriter.dll',
    'System.Xml.Serialization.dll',
    'System.Xml.XDocument.dll',
    'System.Xml.XmlDocument.dll',
    'System.Xml.XmlSerializer.dll',
    'System.Xml.XPath.dll',
    'System.Xml.XPath.XDocument.dll',
    'TraceReloggerLib.dll',
    'WindowsBase.dll',
    'xunit.abstractions.dll',
    'xunit.assert.dll',
    'xunit.core.dll',
    'xunit.execution.dotnet.dll',
    'xunit.performance.core.dll',
    'xunit.performance.execution.dll',
    'xunit.performance.metrics.dll',
    'xunit.runner.utility.dotnet.dll']

class CrossGenRunner:
    def __init__(self, crossgen_executable_filename, no_logo=True):
        self.crossgen_executable_filename = crossgen_executable_filename
        self.no_logo = no_logo
        self.platform_assemblies_paths_sep = ";"

    def run(self, in_filename, out_filename, platform_assemblies_paths):
        p = subprocess.Popen(self._build_args(in_filename, out_filename, platform_assemblies_paths), stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        stdout, stderr = p.communicate()
        return (p.returncode, stdout.decode(), stderr.decode())

    def _build_args(self, in_filename, out_filename, platform_assemblies_paths):
        args = []
        args.append(self.crossgen_executable_filename)
        if self.no_logo:
           args.append('/nologo')
        args.append('/Platform_Assemblies_Paths')
        args.append(self.platform_assemblies_paths_sep.join(platform_assemblies_paths))
        args.append('/out')
        args.append(out_filename)
        args.append(in_filename)
        return args

def compute_file_hashsum(filename):
    algo=hashlib.sha256()
    block_size_in_bytes = 65536
    with open(filename, 'rb') as file:
        while True:
            block = file.read(block_size_in_bytes)
            if block:
                algo.update(block)
            else:
                break
    return algo.hexdigest()

class CrossGenResult:
    def __init__(self, assembly_name, returncode, stdout, stderr, out_file_hashsum):
        self.assembly_name = assembly_name
        self.returncode = returncode
        self.stdout = stdout
        self.stderr = stderr
        self.out_file_hashsum = out_file_hashsum

class CrossGenResultEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, CrossGenResult):
            return {
                'AssemblyName': obj.assembly_name,
                'ReturnCode': obj.returncode,
                'StdOut': obj.stdout.splitlines(),
                'StdErr': obj.stderr.splitlines(),
                'OutputFileHash': obj.out_file_hashsum }
        # Let the base class default method raise the TypeError
        return json.JSONEncoder.default(self, obj)

class CrossGenResultDecoder(json.JSONDecoder):
    def __init__(self, *args, **kwargs):
        json.JSONDecoder.__init__(self, object_hook=self._decode_object, *args, **kwargs)
    def _decode_object(self, dict):
        try:
            assembly_name = dict['AssemblyName']
            returncode = dict['ReturnCode']
            stdout = dict['StdOut']
            stderr = dict['StdErr']
            out_file_hashsum = dict['OutputFileHash']
            return CrossGenResult(assembly_name, returncode, stdout, stderr, out_file_hashsum)
        except KeyError:
            return dict

def crossgen_assembly(crossgen_executable_filename, in_filename, out_filename, platform_assemblies_paths):
    runner = CrossGenRunner(crossgen_executable_filename)
    returncode, stdout, stderr = runner.run(in_filename, out_filename, platform_assemblies_paths)
    assembly_name = os.path.basename(in_filename)
    out_file_hashsum = compute_file_hashsum(out_filename) if returncode == 0 else None
    return CrossGenResult(assembly_name, returncode, stdout, stderr, out_file_hashsum)

def save_crossgen_result_to_json_file(crossgen_result, json_filename):
    with open(json_filename, 'wt') as json_file:
        json.dump(crossgen_result, json_file, cls=CrossGenResultEncoder, indent=2)

def crossgen_corelib(args):
    il_corelib_filename = args.il_corelib_filename
    ni_corelib_filename = os.path.join(tempfile.gettempdir(), os.path.basename(il_corelib_filename))
    crossgen_result = crossgen_assembly(args.crossgen_executable_filename, il_corelib_filename, ni_corelib_filename, [os.path.dirname(il_corelib_filename)])
    result_filename = os.path.join(args.result_dirname, crossgen_result.assembly_name + '.json')
    save_crossgen_result_to_json_file(crossgen_result, result_filename)

def add_ni_extension(filename):
    filename,ext = os.path.splitext(filename)
    return filename + '.ni' + ext

def crossgen_framework(args):
    global g_Framework_Assemblies
    platform_assemblies_paths = [args.core_root]
    for assembly_name in g_Framework_Assemblies:
        il_filename = os.path.join(args.core_root, assembly_name)
        ni_filename = os.path.join(tempfile.gettempdir(), add_ni_extension(assembly_name))
        crossgen_result = crossgen_assembly(args.crossgen_executable_filename, il_filename, ni_filename, platform_assemblies_paths)
        result_filename = os.path.join(args.result_dirname, crossgen_result.assembly_name + '.json')
        save_crossgen_result_to_json_file(crossgen_result, result_filename)

def load_crossgen_result_from_json_file(json_filename):
    with open(json_filename, 'rt') as json_file:
        return json.load(json_file, cls=CrossGenResultDecoder)

def load_crossgen_results_from_dir(dirname):
    results_by_assembly_name = dict()
    for filename in glob.glob(os.path.join(dirname, '*.json')):
        result = load_crossgen_result_from_json_file(filename)
        results_by_assembly_name[result.assembly_name] = result
    return results_by_assembly_name

def compare(args):
    base_results = load_crossgen_results_from_dir(args.base_dirname)
    diff_results = load_crossgen_results_from_dir(args.diff_dirname)

    base_assemblies = set(base_results.keys())
    diff_assemblies = set(diff_results.keys())

    column_width = max(len(assembly_name) for assembly_name in base_assemblies | diff_assemblies)
    has_mismatch_error = False

    for assembly_name in sorted(base_assemblies & diff_assemblies):
        base_result = base_results[assembly_name]
        diff_result = diff_results[assembly_name]

        if base_result.out_file_hashsum == diff_result.out_file_hashsum:
            print('{0}  [OK]'.format(assembly_name.ljust(column_width)))
        else:
            print('{0}  [MISMATCH]'.format(assembly_name.ljust(column_width)))
            has_mismatch_error = True

    for assembly_name in sorted(base_assemblies - diff_assemblies):
        print('{0}  [BASE ONLY]'.format(assembly_name.ljust(column_width)))
        has_mismatch_error = True

    for assembly_name in sorted(diff_assemblies - base_assemblies):
        print('{0}  [DIFF ONLY]'.format(assembly_name.ljust(column_width)))
        has_mismatch_error = True

    sys.exit(1 if has_mismatch_error else 0)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers()

    corelib_parser = subparsers.add_parser('crossgen_corelib')
    corelib_parser.add_argument('--crossgen', dest='crossgen_executable_filename')
    corelib_parser.add_argument('--il_corelib', dest='il_corelib_filename')
    corelib_parser.add_argument('--result_dir', dest='result_dirname')
    corelib_parser.set_defaults(func=crossgen_corelib)

    frameworks_parser = subparsers.add_parser('crossgen_framework')
    frameworks_parser.add_argument('--crossgen', dest='crossgen_executable_filename')
    frameworks_parser.add_argument('--core_root', dest='core_root')
    frameworks_parser.add_argument('--result_dir', dest='result_dirname')
    frameworks_parser.set_defaults(func=crossgen_framework)

    compare_parser = subparsers.add_parser('compare')
    compare_parser.add_argument('--base_dir', dest='base_dirname')
    compare_parser.add_argument('--diff_dir', dest='diff_dirname')
    compare_parser.set_defaults(func=compare)

    args = parser.parse_args()
    func = args.func(args)
