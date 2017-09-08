@Library('dotnet-ci') _

// Incoming parameters.  Access with "params.<param name>".
// Note that the parameters will be set as env variables so we cannot use names that conflict
// with the engineering system parameter names.

//--------------------- Windows Functions ----------------------------//

def windowsBuild(String arch, String config, String pgo, boolean isBaseline) {
    checkout scm

    String pgoStash = ((pgo == "skiprestoreoptdata") ? "nopgo" : "pgo")
    String baselineString = ""

    // For baseline builds, checkout the merge's parent
    if (isBaseline) {
        baselineString = "-baseline"
        bat "git checkout HEAD^^1"
    }

    bat "set __TestIntermediateDir=int&&.\\build.cmd ${config} ${arch} skipbuildpackages ${pgo}"

    // Stash build artifacts. Stash tests in an additional stash to be used by Linux test runs
    stash name: "nt-${arch}-${pgoStash}${baselineString}-build-artifacts", includes: 'bin/**'
    stash name: "nt-${arch}-${pgoStash}${baselineString}-test-artifacts", includes: 'bin/tests/**'
}

// TODO: need to add the SAS token
def windowsPerf(String arch, String config, String uploadString, String runType, String opt_level, String jit, String pgo, String scenario, boolean isBaseline) {
    checkout scm
    String baselineString = ""
    if (isBaseline) {
        baselineString = "-baseline"
    }
    dir ('.') {
        unstash "nt-${arch}-${pgo}${baselineString}-build-artifacts"
        unstash "benchview-artifacts"
        unstash "metadata"
    }

    // We want to use the baseline metadata for baseline runs. We expect to find the submission metadata in
    // submission-metadata.py
    if (isBaseline) {
        bat "move /y submission-metadata-baseline.json submission-metadata.json"
    }

    String testEnv = ""

    if (jit == 'legacy_backend') {
        testEnv = "-testEnv %WORKSPACE%\\tests\\legacyjit_x86_testenv.cmd"
    }

    String failedOutputLogFilename = "run-xunit-perf-scenario.log"

    bat "py \".\\Microsoft.BenchView.JSONFormat\\tools\\machinedata.py\""
    bat ".\\init-tools.cmd"
    bat "run.cmd build -Project=\"tests\\build.proj\" -BuildOS=Windows_NT -BuildType=${config} -BuildArch=${arch} -BatchRestorePackages"
    bat "tests\\runtest.cmd ${config} ${arch} GenerateLayoutOnly"

    // We run run-xunit-perf differently for each of the different job types
    if (scenario == 'perf') {
        String runXUnitPerfCommonArgs = "-arch ${arch} -configuration ${config} -generateBenchviewData \"%WORKSPACE%\\Microsoft.Benchview.JSONFormat\\tools\" ${uploadString} -runtype ${runType} ${testEnv} -optLevel ${opt_level} -jitName ${jit} -stabilityPrefix \"START \"CORECLR_PERF_RUN\" /B /WAIT /HIGH /AFFINITY 0x2\""
        bat "tests\\scripts\\run-xunit-perf.cmd ${runXUnitPerfCommonArgs} -testBinLoc bin\\tests\\${os}.${arch}.${config}\\performance\\perflab\\Perflab -library"
        bat "tests\\scripts\\run-xunit-perf.cmd ${runXUnitPerfCommonArgs} -testBinLoc bin\\tests\\${os}.${arch}.${config}\\Jit\\Performance\\CodeQuality"

        bat "tests\\scripts\\run-xunit-perf.cmd ${runXUnitPerfCommonArgs} -testBinLoc bin\\tests\\${os}.${arch}.${config}\\performance\\perflab\\Perflab -library -collectionFlags default+BranchMispredictions+CacheMisses+InstructionRetired+gcapi"
        bat "tests\\scripts\\run-xunit-perf.cmd ${runXUnitPerfCommonArgs} -testBinLoc bin\\tests\\${os}.${arch}.${config}\\Jit\\Performance\\CodeQuality -collectionFlags default+BranchMispredictions+CacheMisses+InstructionRetired+gcapi"
    }
    else if (scenario == 'jitbench') {
        String runXUnitPerfCommonArgs = "-arch ${arch} -configuration ${config} -generateBenchviewData \"%WORKSPACE%\\Microsoft.Benchview.JSONFormat\\tools\" ${uploadString} -runtype ${runType} ${testEnv} -optLevel ${opt_level} -jitName ${jit} -scenarioTest"
        bat "tests\\scripts\\run-xunit-perf.cmd ${runXUnitPerfCommonArgs} -testBinLoc bin\\tests\\${os}.${arch}.${config}\\performance\\Scenario\\JitBench -group CoreCLR-Scenarios || (echo [ERROR] JitBench failed. 1>>\"${failedOutputLogFilename}\" && exit /b 1)"
    }
    else if (scenario == 'illink') {
        String runXUnitPerfCommonArgs = "-arch ${arch} -configuration ${config} -generateBenchviewData \"%WORKSPACE%\\Microsoft.Benchview.JSONFormat\\tools\" ${uploadString} -runtype ${runType} ${testEnv} -optLevel ${opt_level} -jitName ${jit} -scenarioTest"
        bat "tests\\scripts\\run-xunit-perf.cmd ${runXUnitPerfCommonArgs} -testBinLoc bin\\tests\\${os}.${arch}.${config}\\performance\\linkbench\\linkbench -group ILLink -nowarmup || (echo [ERROR] IlLink failed. 1>>\"${failedOutputLogFilename}\" && exit /b 1)"
    }
    archiveArtifacts allowEmptyArchive: false, artifacts:'Perf-*.xml,Perf-*.etl,Perf-*.log,machinedata.json'
}

def windowsThroughput(String arch, String os, String config, String runType, String optLevel, String jit, String pgo, boolean isBaseline) {
    checkout scm
    
    String baselineString = ""
    if (isBaseline) {
        baselineString = "-baseline"
    }
    
    dir ('.') {
        unstash "nt-${arch}-${pgo}${baselineString}-build-artifacts"
        unstash "benchview-artifacts"
        unstash "throughput-benchmarks-${arch}"
        unstash "metadata"
    }

    // We want to use the baseline metadata for baseline runs. We expect to find the submission metadata in
    // submission-metadata.py
    if (isBaseline) {
        bat "move /y submission-metadata-baseline.json submission-metadata.json"
    }

    bat "py \".\\Microsoft.BenchView.JSONFormat\\tools\\machinedata.py\""
    bat ".\\init-tools.cmd"
    bat "tests\\runtest.cmd ${config} ${arch} GenerateLayoutOnly"
    bat "py -u tests\\scripts\\run-throughput-perf.py -arch ${arch} -os ${os} -configuration ${config} -opt_level ${optLevel} -jit_name ${jit} -clr_root \"%WORKSPACE%\" -assembly_root \"%WORKSPACE%\\${arch}ThroughputBenchmarks\\lib\" -benchview_path \"%WORKSPACE%\\Microsoft.Benchview.JSONFormat\\tools\" -run_type ${runType}"
    archiveArtifacts allowEmptyArchive: false, artifacts:'throughput-*.csv,machinedata.json'
}

//------------------------ Linux Functions ----------------------------//

def linuxBuild(String arch, String config, String pgo, boolean isBaseline) {
    checkout scm

    String pgoStash = ((pgo == "skiprestoreoptdata") ? "nopgo" : "pgo")
    String baselineString = ""

    // For baseline runs, checkout the merge's parent
    if (isBaseline) {
        baselineString = "-baseline"
        sh "git checkout HEAD^1"
    }

    sh "./build.sh verbose ${config} ${arch} ${pgo}"
    stash name: "linux-${arch}-${pgoStash}${baselineString}-build-artifacts", includes: 'bin/**'
}

def linuxPerf(String arch, String os, String config, String uploadString, String runType, String optLevel, String pgo, boolean isBaseline) {
    checkout scm

    String baselineString = ""
    if (isBaseline) {
        baselineString = "-baseline"
    }

    dir ('.') {
        unstash "linux-${arch}-${pgo}${baselineString}-build-artifacts"
        unstash "nt-${arch}-${pgo}${baselineString}-test-artifacts"
        unstash "benchview-artifacts"
        unstash "metadata"
    }

    // We want to use the baseline metadata for baseline runs. We expect to find the submission metadata in
    // submission-metadata.py
    if (isBaseline) {
        sh "mv submission-metadata-baseline.json submission-metadata.json"
    }

    sh "./tests/scripts/perf-prep.sh"
    sh "./init-tools.sh"
    sh "./tests/scripts/run-xunit-perf.sh --testRootDir=\"\${WORKSPACE}/bin/tests/Windows_NT.${arch}.${config}\" --testNativeBinDir=\"\${WORKSPACE}/bin/obj/Linux.${arch}.${config}/tests\" --coreClrBinDir=\"\${WORKSPACE}/bin/Product/Linux.${arch}.${config}\" --mscorlibDir=\"\${WORKSPACE}/bin/Product/Linux.${arch}.${config}\" --coreFxBinDir=\"\${WORKSPACE}/corefx\" --runType=\"${runType}\" --benchViewOS=\"${os}\" --stabilityPrefix=\"taskset 0x00000002 nice --adjustment=-10\" --uploadToBenchview --generatebenchviewdata=\"\${WORKSPACE}/tests/scripts/Microsoft.BenchView.JSONFormat/tools\""
    archiveArtifacts allowEmptyArchive: false, artifacts:'Perf-*.xml,Perf-*.log,machinedata.json'
}

def linuxThroughput(String arch, String os, String config, String uploadString, String runType, String optLevel, String pgo, boolean isBaseline) {
    checkout scm

    String baselineString = ""
    if (isBaseline) {
        baselineString = "-baseline"
    }

    dir ('.') {
        unstash "linux-${arch}-${pgo}${baselineString}-build-artifacts"
        unstash "benchview-artifacts"
        unstash "throughput-benchmarks-${arch}"
        unstash "metadata"
    }

    // We want to use the baseline metadata for baseline runs. We expect to find the submission metadata in
    // submission-metadata.py
    if (isBaseline) {
        sh "mv submission-metadata-baseline.json submission-metadata.json"
    }

    sh "./tests/scripts/perf-prep.sh --throughput"
    sh "./init-tools.sh"
    sh "python3 ./tests/scripts/run-throughput-perf.py -arch \"${arch}\" -os \"${os}\" -configuration \"${config}\" -opt_level ${optLevel} -clr_root \"\${WORKSPACE}\" -assembly_root \"\${WORKSPACE}/${arch}ThroughputBenchmarks/lib\" -run_type \"${runType}\"  -benchview_path \"\${WORKSPACE}/tests/scripts/Microsoft.BenchView.JSONFormat/tools\""
    archiveArtifacts allowEmptyArchive: false, artifacts:'throughput-*.csv,machinedata.json'
}

//-------------------------- Job Definitions --------------------------//

String config = "Release"
String runType = isPR() ? 'private' : 'rolling'

// TODO: This needs to be updated before going live
String uploadString = ''

stage ('Get Metadata and download Throughput Benchmarks') {
    simpleNode('Windows_NT', '20170427-elevated') {
        checkout scm
        String commit = getCommit()
        def benchViewName = isPR() ? "coreclr private %ghprbPullTitle%" : "coreclr rolling %GIT_BRANCH_WITHOUT_ORIGIN% ${commit}"
        def benchViewUser = getUserEmail()
        bat "mkdir tools\n" +
            "powershell Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/v4.1.0/nuget.exe -OutFile %WORKSPACE%\\tools\\nuget.exe"
        bat "%WORKSPACE%\\tools\\nuget.exe install Microsoft.BenchView.JSONFormat -Source http://benchviewtestfeed.azurewebsites.net/nuget -Prerelease -ExcludeVersion"
        bat "%WORKSPACE%\\tools\\nuget.exe install Microsoft.BenchView.ThroughputBenchmarks.x64.Windows_NT -Source https://dotnet.myget.org/F/dotnet-core -Prerelease -ExcludeVersion"
        bat "%WORKSPACE%\\tools\\nuget.exe install Microsoft.BenchView.ThroughputBenchmarks.x86.Windows_NT -Source https://dotnet.myget.org/F/dotnet-core -Prerelease -ExcludeVersion"
        bat "set \"GIT_BRANCH_WITHOUT_ORIGIN=%GitBranchOrCommit:*/=%\"\n" +
            "py \"%WORKSPACE%\\Microsoft.BenchView.JSONFormat\\tools\\submission-metadata.py\" --name \"${benchViewName}\" --user \"${benchViewUser}\"\n" +
            "py \"%WORKSPACE%\\Microsoft.BenchView.JSONFormat\\tools\\build.py\" git --branch %GIT_BRANCH_WITHOUT_ORIGIN% --type ${runType}\n" +
            "py \"%WORKSPACE%\\Microsoft.BenchView.JSONFormat\\tools\\submission-metadata.py\" --name \"${benchViewName}-baseline\" --user \"${benchViewUser}\" -o submission-metadata-baseline.json\n"
        
        // TODO: revisit these moves. Originally, stash could not find the directories as currently named
        bat "move Microsoft.BenchView.ThroughputBenchmarks.x64.Windows_NT x64ThroughputBenchmarks"
        bat "move Microsoft.BenchView.ThroughputBenchmarks.x86.Windows_NT x86ThroughputBenchmarks"

        stash name: 'benchview-artifacts', includes: 'Microsoft.BenchView.JSONFormat/**/*'
        stash name: 'metadata', includes: '*.json'
        stash name: 'throughput-benchmarks-x64', includes: 'x64ThroughputBenchmarks/**/*'
        stash name: 'throughput-benchmarks-x86', includes: 'x86ThroughputBenchmarks/**/*'
    }
}

// TODO: use non-pgo builds for throughput?
def innerLoopBuilds = [
    "windows x64 pgo build": {
        simpleNode('Windows_NT','latest') {
            windowsBuild('x64', config, '', false)
        }
     },
    "windows x86 pgo build": {
        simpleNode('Windows_NT','latest') {
            windowsBuild('x86', config, '', false)
        }
    },
    "linux x64 pgo build": {
        simpleNode('Ubuntu16.04','latest') {
            linuxBuild('x64', config, '', false)
        }
    }
]

// Only run non-pgo builds on offical builds
def outerLoopBuilds = [:]

if (!isPR()) {
    outerLoopBuilds = [
        "windows x64 nopgo build": {
            simpleNode('Windows_NT','latest') {
                windowsBuild('x64', config, 'skiprestoreoptdata', false)
            }
        },
        "windows x86 nopgo build": {
           simpleNode('Windows_NT','latest') {
               windowsBuild('x86', config, 'skiprestoreoptdata', false)
           }
        },
        "linux x64 nopgo build": {
           simpleNode('Ubuntu16.04','latest') {
               linuxBuild('x64', config, 'skiprestoreoptdata', false)
           }
        }
    ]
}

def baselineBuilds = [:]

if (isPR()) {
   baselineBuilds = [
       "windows x64 pgo baseline build": {
           simpleNode('Windows_NT','latest') {
               windowsBuild('x64', config, '', true)
           }
       },
       "windows x86 pgo baseline build": {
           simpleNode('Windows_NT','latest') {
               windowsBuild('x86', config, '', true)
           }
       },
       "linux x64 pgo baseline build": {
           simpleNode('Ubuntu16.04','latest') {
               linuxBuild('x64', config, '', true)
           }
       }
   ]
}

stage ('Build Product') {
    parallel innerLoopBuilds + outerLoopBuilds + baselineBuilds
}

// Pipeline builds don't allow outside scripts (ie ArrayList.Add) if running from a script from SCM, so manually list these for now.
// Run the main test mix on all runs (PR + official)

def innerLoopTests = [:]

['x64', 'x86'].each { arch ->
    [true,false].each { isBaseline ->
        String baseline = ""
        if (isBaseline) {
            baseline = " baseline"
        }
        if (isPR() || !isBaseline) {
            innerLoopTests["windows ${arch} ryujit full_opt pgo${baseline} perf"] = {
               simpleNode('Windows_NT','20170427-elevated') {
                   windowsPerf(arch, config, uploadString, runType, 'full_opt', 'ryujit', 'pgo', 'perf', isBaseline)
               }
            }

            innerLoopTests["windows ${arch} ryujit full_opt pgo${baseline} throughput"] = {
               simpleNode('Windows_NT','20170427-elevated') {
                   windowsThroughput(arch, 'Windows_NT', config, runType, 'full_opt', 'ryujit', 'pgo', isBaseline)
               }
            }

            innerLoopTests["windows ${arch} ryujit full_opt pgo${baseline} jitbench"] = {
                simpleNode('Windows_NT','20170427-elevated') {
                    windowsPerf(arch, config, uploadString, runType, 'full_opt', 'ryujit', 'pgo', 'jitbench', isBaseline)
                }
            }

            innerLoopTests["windows ${arch} ryujit full_opt pgo${baseline} illink"] = {
               simpleNode('Windows_NT','20170427-elevated') {
                   windowsPerf(arch, config, uploadString, runType, 'full_opt', 'ryujit', 'pgo', 'illink', isBaseline)
               }
            }

            if (arch == 'x64') {
               innerLoopTests["linux ${arch} ryujit full_opt pgo${baseline} perf"] = {
                   simpleNode('Ubuntu','latest') {
                       linuxPerf('x64', 'Ubuntu14.04', config, uploadString, runType, 'full_opt', 'pgo', isBaseline)
                   }
               }

               innerLoopTests["linux ${arch} ryujit full_opt pgo${baseline} throughput"] = {
                   simpleNode('Ubuntu','latest') {
                       linuxThroughput('x64', 'Ubuntu14.04', config, uploadString, runType, 'full_opt', 'pgo', isBaseline)
                   }
               }
            }
        }
    }
}

// Run the full test mix only on commits, not PRs
def outerLoopTests = [:]

if (!isPR()) {
    ['x64', 'x86'].each { arch ->
        ['min_opt', 'full_opt'].each { opt_level ->
            ['ryujit', 'legacy_backend'].each { jit ->
                ['pgo', 'nopgo'].each { pgo_enabled ->
                    if (!(arch == 'x64' && jit == 'legacy_backend'))
                    {
                        if (!(pgo_enabled == 'nopgo' && opt_level == 'min_opt'))
                        {
                            outerLoopTests["windows ${arch} ${jit} ${opt_level} ${pgo_enabled} perf"] = {
                                simpleNode('Windows_NT','20170427-elevated') {
                                    windowsPerf(arch, config, uploadString, runType, opt_level, jit, pgo_enabled, 'perf', false)
                                }
                            }

                            outerLoopTests["windows ${arch} ${jit} ${opt_level} ${pgo_enabled} throughput"] = {
                                simpleNode('Windows_NT','20170427-elevated') {
                                    windowsThroughput(arch, 'Windows_NT', config, runType, opt_level, jit, pgo_enabled, false)
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    ['x64'].each { arch ->
        ['min_opt', 'full_opt'].each { opt_level ->
            ['pgo', 'nopgo'].each { pgo_enabled ->
                if (!(pgo_enabled == 'nopgo' && opt_level == 'min_opt'))
                {
                    outerLoopTests["linux ${arch} ryujit ${opt_level} ${pgo_enabled} perf"] = {
                        simpleNode('Ubuntu','latest') {
                            linuxPerf(arch, 'Ubuntu14.04', config, uploadString, runType, opt_level, pgo_enabled, false)
                        }
                    }

                    outerLoopTests["linux ${arch} ryujit ${opt_level} ${pgo_enabled} throughput"] = {
                        simpleNode('Ubuntu','latest') {
                            linuxThroughput(arch, 'Ubuntu14.04', config, uploadString, runType, opt_level, pgo_enabled, false)
                        }
                    }
                }
            }
        }
    }
}

stage ('Run testing') {
    parallel innerLoopTests + outerLoopTests
}
