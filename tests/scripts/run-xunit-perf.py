import json
import argparse
import os
import shutil
import subprocess
import sys
import glob

##########################################################################
# Argument Parser
##########################################################################

description = 'Tool to run coreclr perf tests'

parser = argparse.ArgumentParser(description=description)

parser.add_argument('-testBinLoc', dest='coreclrPerf', default=None, required=True)
parser.add_argument('-arch', dest='arch', default='x64', choices=['x64', 'x86'])
parser.add_argument('-os', dest='operatingSystem', default=sys.platform, choices=['Windows_NT', 'Ubuntu16.04', 'Ubuntu14.04', 'OSX', sys.platform])
parser.add_argument('-configuration', dest='configuration', default='Release', choices=['Release', 'Checked', 'Debug'])
parser.add_argument('-optLevel', dest='optLevel', default='full_opt', choices=['full_opt', 'min_opt', 'tiered'])
parser.add_argument('-jitName', dest='jitName', default='ryujit')
parser.add_argument('-runtype', dest='runType', default='local', choices=['local', 'private', 'rolling'])
parser.add_argument('-noPgo', dest='isPgoOptimized', action='store_false', default=True)
parser.add_argument('-scenarioTest', dest='isScenarioTest', action='store_true', default=False)
parser.add_argument('-stabilityPrefix', dest='stabilityPrefix', default=None)
parser.add_argument('-uploadToBenchview', dest='uploadToBenchview', action='store_true', default=False)
parser.add_argument('-generateBenchviewData', dest='benchviewPath', default=None)
parser.add_argument('-slice', dest='sliceNumber', default=-1, type=int)
parser.add_argument('-sliceConfigFile', dest='sliceConfigFile', default=None)
parser.add_argument('-nowarmup', dest='hasWarmupRun', action='store_false', default=True)
parser.add_argument('-better', dest='better', default='desc')
parser.add_argument('-collectionFlags', dest='collectionFlags', default='stopwatch')
parser.add_argument('-library', dest='library', action='store_true', default=False)
parser.add_argument('-group', dest='benchviewGroup', default='CoreCLR')
parser.add_argument('-outputdir', dest='outputDir', default=None)

##########################################################################
# Helper Functions
##########################################################################

def validate_args(args):
    """ Validate all of the arguments parsed.
    Args:
        args (argparser.ArgumentParser): Args parsed by the argument parser.
    Returns:
        (arch, build_type, clr_root, fx_root, fx_branch, fx_commit, env_script)
            (str, str, str, str, str, str, str)
    Notes:
    If the arguments are valid then return them all in a tuple. If not, raise
    an exception stating x argument is incorrect.
    """

    def validate_arg(arg, check):
        """ Validate an individual arg
        Args:
           arg (str|bool): argument to be validated
           check (lambda: x-> bool): test that returns either True or False
                                   : based on whether the check passes.

        Returns:
           is_valid (bool): Is the argument valid?
        """

        helper = lambda item: item is not None and check(item)

        if not helper(arg):
            raise ValueError('Argument: %s is not valid.' % (arg))

    coreclrPerf = os.path.join(os.getcwd(), args.coreclrPerf)
    validate_arg(coreclrPerf, lambda item: os.path.isdir(item))

    if not args.benchviewPath is None:
        validate_arg(args.benchviewPath, lambda item: os.path.isdir(item))
    if not args.sliceNumber == -1:
        validate_arg(args.sliceConfigFile, lambda item: os.path.isfile(item))
    if not args.outputDir is None:
        validate_arg(args.outputDir, lambda item: os.path.isdir(item))

    log('Args:')
    log('arch: %s' % args.arch)
    log('operatingSystem: %s' % args.operatingSystem)
    log('jitName: %s' % args.jitName)
    log('optLevel: %s' % args.optLevel)
    log('coreclrPerf: %s' % coreclrPerf)
    log('better: %s' % args.better)
    log('runType: %s' % args.runType)
    log('configuration: %s' % args.configuration)
    if not args.benchviewPath is None:
        log('benchviewPath: %s' % args.benchviewPath)
    if not args.sliceNumber == -1:
        log('sliceNumber: %s' % args.sliceNumber)
        log('sliceConfigFile: %s' % args.sliceConfigFile)
    if not outputDir is None:
        log('outputDir: %s' % args.outputDir)
    if not stabilityPrefix is None:
        log('stabilityPrefix: %s' % args.stabilityPrefix)
    log('isScenarioTest: %s' % args.isScenarioTest)
    log('isPgoOptimized: %s' % args.isPgoOptimized)
    log('benchviewGroup: %s' % args.isScenarioTest)
    log('hasWarmupRun: %s' % args.hasWarmupRun)
    log('library: %s' % args.library)
    log('collectionFlags: %s' % args.collectionFlags)
    log('uploadToBenchview: %s' % args.uploadToBenchview)

    return (coreclrPerf, args.arch, args.operatingSystem, args.configuration, args.jitName, args.optLevel, args.runType, args.outputDir, args.stabilityPrefix, args.isScenarioTest, args.benchviewPath, args.isPgoOptimized, args.benchviewGroup, args.hasWarmupRun, args.collectionFlags, args.library, args.uploadToBenchview, args.better, args.sliceNumber, args.sliceConfigFile)

def log(message):
    """ Print logging information
    Args:
        message (str): message to be printed
    """

    print('[%s]: %s' % (sys.argv[0], message))

def print_error(out, err, message):
    log('%s' % out.decode("utf-8"))
    log('%s' % err.decode("utf-8"))
    raise Exception(message)

def run_benchmark(benchname, benchdir, env, sandboxDir, benchmarkOutputDir, testFileExt, stabilityPrefix, collectionFlags, lvRunId, etwCollection, isScenarioTest, arch, extension, executable):
    my_env = env
    benchnameWithExt = benchname + '.' + testFileExt
    fullPath = os.path.join(benchdir, benchnameWithExt)
    shutil.copy2(fullPath, sandboxDir)

    files = glob.iglob(os.path.join(benchdir, "*.txt"))
    for filename in files:
        if os.path.isfile(filename):
            shutil.copy2(filename, sandboxDir)

    my_env['CORE_ROOT'] = sandboxDir

    benchnameLogFileName = os.path.join(benchmarkOutputDir, lvRunId + '-' + benchname + '.log')

    if not os.path.isdir(benchmarkOutputDir):
        os.makedirs(benchmarkOutputDir)

    log('')
    log('--------')
    log('Running %s %s' % (lvRunId, benchname))
    log('--------')
    log('')

    lvCommonArgs = [os.path.join(sandboxDir, benchnameWithExt),
            '--perf:outputdir',
            benchmarkOutputDir,
            '--perf:runid',
            lvRunId]

    if isScenarioTest:
        lvCommonArgs += ['--target-architecture',
                arch]
    else:
        lvCommonArgs.insert(0, 'PerfHarness.dll')

    splitPrefix = [] if stabilityPrefix is None else stabilityPrefix.split(' ')
    run_args = executable + splitPrefix + [os.path.join(sandboxDir, 'corerun' + extension)] + lvCommonArgs + ['--perf:collect', collectionFlags]
    log(" ".join(run_args))
    sys.stdout.flush()

    error = 0
    expectedOutputFile = os.path.join(benchmarkOutputDir, lvRunId + '-' + benchname + '.xml')
    with open(benchnameLogFileName, 'wb') as out:
        proc = subprocess.Popen(' '.join(run_args), shell=True, stdout=out, stderr=out, env=my_env)
        proc.communicate()
        if not proc.returncode == 0:
            error = proc.returncode

    if not error == 0:
        log("CoreRun.exe exited with %s code" % (error))
        if os.path.isfile(benchnameLogFileName):
            with open(benchnameLogFileName, 'r') as f:
                print(f.read())
        return error
    elif not os.path.isfile(expectedOutputFile):
        log("CoreRun.exe failed to generate results in %s." % expectedOutputFile)
        return 1

    return 0

def generate_results_for_benchview(python, lvRunId, benchname, isScenarioTest, better, hasWarmupRun, benchmarkOutputDir, benchviewPath):
    benchviewMeasurementParser = 'xunitscenario' if isScenarioTest else 'xunit'
    warmupRun = '--drop-first-value' if hasWarmupRun else ''
    lvMeasurementArgs = [benchviewMeasurementParser,
            '--better',
            better,
            warmupRun,
            '--append']

    filename = os.path.join(benchmarkOutputDir, lvRunId + '-' + benchname + '.xml')

    run_args = [python, os.path.join(benchviewPath, 'measurement.py')] + lvMeasurementArgs + [filename]
    log('')
    log(" ".join(run_args))

    proc = subprocess.Popen(run_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    (out, err) = proc.communicate()
    if not proc.returncode == 0:
        print_error(out, err, 'Call to measurement.py failed.')

def upload_to_benchview(python, coreclrRepo, benchviewPath, uploadToBenchview, benchviewGroup, runType, configuration, operatingSystem, etwCollection, optLevel, jitName, pgoOptimized, architecture):
    measurementJson = os.path.join(os.getcwd(), 'measurement.json')
    buildJson = os.path.join(coreclrRepo, 'build.json')
    machinedataJson = os.path.join(coreclrRepo, 'machinedata.json')
    submissionMetadataJson = os.path.join(coreclrRepo, 'submission-metadata.json')

    if not os.path.isfile(measurementJson):
        raise Exception('%s does not exist. There is no data to be uploaded.' % measurementJson)
    if not os.path.isfile(buildJson):
        raise Exception('%s does not exist. There is no data to be uploaded.' % buildJson)
    if not os.path.isfile(machinedataJson):
        raise Exception('%s does not exist. There is no data to be uploaded.' % machinedataJson)
    if not os.path.isfile(submissionMetadataJson):
        raise Exception('%s does not exist. There is no data to be uploaded.' % submissionMetadataJson)

    run_args = [python,
            os.path.join(benchviewPath, 'submission.py'),
            measurementJson,
            '--build',
            buildJson,
            '--machine-data',
            machinedataJson,
            '--metadata',
            submissionMetadataJson,
            '--group',
            benchviewGroup,
            '--type',
            runType,
            '--config-name',
            configuration,
            '--config',
            'OS',
            operatingSystem,
            '--config',
            'Profile',
            etwCollection,
            '--config',
            'OptLevel',
            optLevel,
            '--config',
            'JitName',
            jitName,
            '--config',
            'PGO',
            pgoOptimized,
            '--architecture',
            architecture,
            '--machinepool',
            'PerfSnake']
    log('')
    log(" ".join(run_args))

    proc = subprocess.Popen(run_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    (out, err) = proc.communicate()

    if not proc.returncode == 0:
        print_error(out, err, 'Createing BenchView submission data failed.')

    if uploadToBenchview:
        run_args = [python,
                os.path.join(benchviewPath, 'upload.py'),
                'submission.json',
                '--container',
                'coreclr']
        log(" ".join(run_args))
        proc = subprocess.Popen(run_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        (out, err) = proc.communicate()

        if not proc.returncode == 0:
            print_error(out, err, 'Uploading to BenchView failed.')

def verify_core_overlay(coreclrRepo, operatingSystem, arch, configuration):
    configurationDir = "%s.%s.%s" % (operatingSystem, arch, configuration)
    overlayDir = 'Core_Root'
    coreclrOverlay = os.path.join(coreclrRepo, "bin", "tests", configurationDir, "Tests", overlayDir)
    if not os.path.isdir(coreclrOverlay):
        raise ValueError("Can't find test overlay directory '%s'. Please build and run CoreCLR tests" % coreclrOverlay)

    return coreclrOverlay

def setup_sandbox(sandboxDir):
    if os.path.isdir(sandboxDir):
        shutil.rmtree(sandboxDir)
    os.mkdir(sandboxDir)

def set_perf_run_log(sandboxOutputDir):
    if not os.path.isdir(sandboxOutputDir):
        os.mkdir(sandboxOutputDir)
    return os.path.join(sandboxOutputDir, "perfrun.log")

def setup_optimization_level(env, optLevel):
    my_env = env
    if optLevel == 'min_opts':
        my_env['COMPlus_JITMinOpts'] = '1'
    elif optLevel == 'tiered':
        my_env['COMPLUS_EXPERIMENTAL_TieredCompilation'] = '1'

    return my_env

def build_perfharness(coreclrRepo, sandboxDir, extension, dotnetEnv):
    # Confirm dotnet works
    dotnet = os.path.join(coreclrRepo, 'Tools', 'dotnetcli', 'dotnet' + extension)
    run_args = [dotnet,
            '--info'
            ]
    log(" ".join(run_args))

    proc = subprocess.Popen(run_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=dotnetEnv)
    (out, err) = proc.communicate()

    if not proc.returncode == 0:
        print_error(out, err, 'Failed to get information about the CLI tool.')

    # Restore PerfHarness
    perfHarnessPath = os.path.join(coreclrRepo, 'tests', 'src', 'Common', 'PerfHarness', 'PerfHarness.csproj')
    run_args = [dotnet,
            'restore',
            perfHarnessPath]
    log(" ".join(run_args))

    proc = subprocess.Popen(run_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=dotnetEnv)
    (out, err) = proc.communicate()

    if not proc.returncode == 0:
        print_error(out, err, 'Failed to restore PerfHarness.csproj')

    # Publish PerfHarness
    run_args = [dotnet,
            'publish',
            perfHarnessPath,
            '-c',
            'Release',
            '-o',
            sandboxDir]
    log(" ".join(run_args))

    proc = subprocess.Popen(run_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=dotnetEnv)
    (out, err) = proc.communicate()

    if not proc.returncode == 0:
        print_error(out, err, 'Failed to publish PerfHarness.csproj')

def copytree(src, dst):
    for item in os.listdir(src):
        s = os.path.join(src, item)
        d = os.path.join(dst, item)
        if os.path.isdir(s):
            shutil.copytree(s, d)
        else:
            shutil.copy2(s, d)

def main(args):
    coreclrPerf, arch, operatingSystem, configuration, jitName, optLevel, runType, outputDir, stabilityPrefix, isScenarioTest, benchviewPath, isPgoOptimized, benchviewGroup, hasWarmupRun, collectionFlags, isLibrary, uploadToBenchview, better, sliceNumber, sliceConfigFile = validate_args(args)

    platform = sys.platform
    python = 'py'
    if platform == 'linux' or platform == 'linux2':
        platform = 'Linux'
        python = 'python3'
    elif platform == 'darwin':
        platform = 'OSX'
        python = 'python3'
    elif platform == 'win32':
        platform = "Windows_NT"

    executable = ['cmd.exe', '/c'] if platform == 'Windows_NT' else []

    coreclrRepo = os.getcwd()
    etwCollection = 'Off' if collectionFlags == 'stopwatch' else 'On'
    sandboxDir = os.path.join(coreclrRepo, 'bin', 'sandbox')
    sandboxOutputDir = outputDir if not outputDir is None else os.path.join(sandboxDir, 'Logs')

    extension = '.exe' if platform == 'Windows_NT' else ''

    my_env = os.environ
    dotnetEnv = os.environ

    dotnetEnv['DOTNET_MULTILEVEL_LOOKUP'] = '0'
    dotnetEnv['UseSharedCompilation'] = 'false'

    # Setup directories
    log('Setting up directories')
    coreclrOverlay = verify_core_overlay(coreclrRepo, platform, arch, configuration)
    setup_sandbox(sandboxDir)

    os.chdir(sandboxDir)

    perfRunLog = set_perf_run_log(sandboxOutputDir)
    build_perfharness(coreclrRepo, sandboxDir, extension, dotnetEnv)

    my_env = setup_optimization_level(my_env, optLevel)

    if not 'XUNIT_PERFORMANCE_MAX_ITERATION' in my_env:
        my_env['XUNIT_PERFORMANCE_MAX_ITERATION'] = '21'
    if not 'XUNIT_PERFORMANCE_MAX_ITERATION_INNER_SPECIFIED' in my_env:
        my_env['XUNIT_PERFORMANCE_MAX_ITERATION_INNER_SPECIFIED'] = '21'

    # Copy core overlay contents to sandbox
    copytree(coreclrOverlay, sandboxDir)

    # Determine benchmarks we will be running
    benchmarks = []
    if not sliceNumber == -1:
        with open(sliceConfigFile) as jsonData:
            data = json.load(jsonData)
            if sliceNumber >= len(data["slices"]):
                raise ValueError('Invalid slice number. %s is greater than the max number of slices %s' % (sliceNumber, len(data["slices"])))
            for benchmark in data["slices"][sliceNumber]["folders"]:
                benchmarks += [benchmark]

    else:
    # If slice was not specified, run everything in the coreclrPerf directory. Set benchmarks to an empty string
        benchmarks = [{ 'directory' : '', 'extraFlags': ''}]

    testFileExt = 'dll' if isLibrary else 'exe'

    # Run benchmarks
    failures = 0
    lvRunId = 'Perf-%s' % etwCollection

    for benchmark in benchmarks:
        testFileExt = 'dll' if benchmark["extraFlags"] == '-library' else 'exe'
        benchmarkDir = os.path.normpath(benchmark["directory"])

        testPath = os.path.join(coreclrPerf, benchmarkDir)

        for root, dirs, files in os.walk(testPath):
            for f in files:
                if f.endswith(testFileExt):
                    benchname, ext = os.path.splitext(f)

                    benchmarkOutputDir = os.path.join(sandboxOutputDir, 'Scenarios') if isScenarioTest else os.path.join(sandboxOutputDir, 'Microbenchmarks')
                    benchmarkOutputDir = os.path.join(benchmarkOutputDir, etwCollection, benchname)

                    failures += run_benchmark(benchname, root, my_env, sandboxDir, benchmarkOutputDir, testFileExt, stabilityPrefix, collectionFlags, lvRunId, etwCollection, isScenarioTest, arch, extension, executable)
                    if not benchviewPath is None:
                        generate_results_for_benchview(python, lvRunId, benchname, isScenarioTest, better, hasWarmupRun, benchmarkOutputDir, benchviewPath)

    # Setup variables for uploading to benchview
    pgoOptimized = 'pgo' if isPgoOptimized else 'nopgo'

    # Upload to benchview
    if not benchviewPath is None:
        upload_to_benchview(python, coreclrRepo, benchviewPath, uploadToBenchview, benchviewGroup, runType, configuration, operatingSystem, etwCollection, optLevel, jitName, pgoOptimized, arch)

    if not failures == 0:
        log('%s benchmarks have failed' % failures)

    return failures

if __name__ == "__main__":
    Args = parser.parse_args(sys.argv[1:])
    sys.exit(main(Args))
