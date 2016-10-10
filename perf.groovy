// Import the utility functionality.

import jobs.generation.*;

def project = GithubProject
def branch = GithubBranchName
def projectName = Utilities.getFolderName(project)
def projectFolder = projectName + '/' + Utilities.getFolderName(branch)

def static getOSGroup(def os) {
    def osGroupMap = ['Ubuntu':'Linux',
        'RHEL7.2': 'Linux',
        'Ubuntu16.04': 'Linux',
        'Debian8.4':'Linux',
        'Fedora23':'Linux',
        'OSX':'OSX',
        'Windows_NT':'Windows_NT',
        'FreeBSD':'FreeBSD',
        'CentOS7.1': 'Linux',
        'OpenSUSE13.2': 'Linux',
        'OpenSUSE42.1': 'Linux',
        'LinuxARMEmulator': 'Linux']
    def osGroup = osGroupMap.get(os, null) 
    assert osGroup != null : "Could not find os group for ${os}"
    return osGroupMap[os]
}
// Setup perflab tests runs
[true, false].each { isPR ->
    ['Windows_NT'].each { os ->
		['x64', 'x86'].each { architecture ->
			def configuration = 'Release'
			def runType = isPR ? 'private' : 'rolling'
			def benchViewName = isPR ? 'coreclr private %ghprbPullTitle%' : 'coreclr rolling %GIT_BRANCH% %GIT_COMMIT%'
			def newJob = job(Utilities.getFullJobName(project, "perf_perflab_${os}", isPR)) {
				// Set the label.
				label('windows_clr_perf')
				wrappers {
					credentialsBinding {
						string('BV_UPLOAD_SAS_TOKEN', 'CoreCLR Perf BenchView Sas')
					}
				}

				steps {
					// Batch
					batchFile("C:\\Tools\\nuget.exe install Microsoft.BenchView.JSONFormat -Source http://benchviewtestfeed.azurewebsites.net/nuget -OutputDirectory C:\\tools -Prerelease -ExcludeVersion")
					batchFile("python C:\\tools\\Microsoft.BenchView.JSONFormat\\tools\\submission-metadata.py --name " + "\"" + benchViewName + "\"" + " --user " + "\"dotnet-bot@microsoft.com\"")
					batchFile("python C:\\tools\\Microsoft.BenchView.JSONFormat\\tools\\build.py git --type " + runType)
					batchFile("python C:\\tools\\Microsoft.BenchView.JSONFormat\\tools\\machinedata.py")
					batchFile("set __TestIntermediateDir=int&&build.cmd release ${architecture}")
					batchFile("tests\\runtest.cmd release ${architecture} GenerateLayoutOnly")
					batchFile("tests\\scripts\\run-xunit-perf.cmd -arch ${architecture} -configuration ${configuration} -testBinLoc bin\\tests\\Windows_NT.x64.Release\\performance\\perflab\\Perflab -library -uploadToBenchview C:\\Tools\\Microsoft.Benchview.JSONFormat\\tools -runtype " + runType)
				}
			}

			// Save machinedata.json to /artifact/bin/ Jenkins dir
			def archiveSettings = new ArchivalSettings()
			archiveSettings.addFiles('sandbox\\perf-*.xml')
			Utilities.addArchival(newJob, archiveSettings)

			Utilities.standardJobSetup(newJob, project, isPR, "*/${branch}")
			
			if (isPR) {
				TriggerBuilder builder = TriggerBuilder.triggerOnPullRequest()
				builder.setGithubContext("${os} CoreCLR Perf Tests")
				builder.triggerOnlyOnComment()
				builder.setCustomTriggerPhrase("(?i).*test\\W+${os}\\W+perf.*")
				builder.triggerForBranch(branch)
				builder.emitTrigger(newJob)
			}
			else {
				// Set a push trigger
				TriggerBuilder builder = TriggerBuilder.triggerOnCommit()
				builder.emitTrigger(newJob)
			}
		}
    }
}

[true, false].each { isPR ->
    ['Windows_NT'].each { os ->
        def architecture = 'x64'
        def configuration = 'Release'
        def newJob = job(Utilities.getFullJobName(project, "perf_${os}", isPR)) {

            // Set the label.
            label('performance')
            steps {
                    // Batch
                    batchFile("C:\\Tools\\nuget.exe install Microsoft.BenchView.JSONFormat -Source http://benchviewtestfeed.azurewebsites.net/nuget -OutputDirectory C:\\tools -Prerelease -ExcludeVersion")
                    batchFile("python C:\\tools\\Microsoft.BenchView.JSONFormat\\tools\\machinedata.py")
                    batchFile("set __TestIntermediateDir=int&&build.cmd release ${architecture}")
                    batchFile("tests\\runtest.cmd release ${architecture}")
                    batchFile("tests\\scripts\\run-xunit-perf.cmd -arch ${architecture} -configuration ${configuration} -testBinLoc bin\\tests\\Windows_NT.x64.Release\\Jit\\Performance\\CodeQuality")
            }
        }

        // Save machinedata.json to /artifact/bin/ Jenkins dir
        def archiveSettings = new ArchivalSettings()
        archiveSettings.addFiles('sandbox\\perf-*.xml')
        archiveSettings.addFiles('machinedata.json')
        Utilities.addArchival(newJob, archiveSettings)

        Utilities.standardJobSetup(newJob, project, isPR, "*/${branch}")

        // For perf, we need to keep the run results longer
        newJob.with {
            // Enable the log rotator
            logRotator {
                artifactDaysToKeep(7)
                daysToKeep(300)
                artifactNumToKeep(25)
                numToKeep(1000)
            }
        }
        if (isPR) {
            TriggerBuilder builder = TriggerBuilder.triggerOnPullRequest()
            builder.setGithubContext("${os} Perf Tests")
            builder.triggerOnlyOnComment()
            builder.setCustomTriggerPhrase("(?i).*test\\W+${os}\\W+perf.*")
            builder.triggerForBranch(branch)
            builder.emitTrigger(newJob)
        }
        else {
            // Set a push trigger
            TriggerBuilder builder = TriggerBuilder.triggerOnCommit()
            builder.emitTrigger(newJob)
        }
    }
}

// Create the Linux/OSX/CentOS coreclr test leg for debug and release and each scenario
[true, false].each { isPR ->
    ['Ubuntu'].each { os ->
        def osGroup = getOSGroup(os)
        def architecture = 'x64'
        def configuration = 'Release'
        def newJob = job(Utilities.getFullJobName(project, "perf_${os}", isPR)) {
            steps {
                shell("sudo bash ./tests/scripts/perf-prep.sh --branch=${projectName}")
                shell("sudo ./init-tools.sh")
                shell("""sudo bash ./tests/scripts/run-xunit-perf.sh \\
                --testRootDir=\"\${WORKSPACE}/bin/tests/Windows_NT.${architecture}.${configuration}\" \\
                --testNativeBinDir=\"\${WORKSPACE}/bin/obj/${osGroup}.${architecture}.${configuration}/tests\" \\
                --coreClrBinDir=\"\${WORKSPACE}/bin/Product/${osGroup}.${architecture}.${configuration}\" \\
                --mscorlibDir=\"\${WORKSPACE}/bin/Product/${osGroup}.${architecture}.${configuration}\" \\
                --coreFxBinDir=\"\${WORKSPACE}/bin/${osGroup}.AnyCPU.${configuration};\${WORKSPACE}/bin/Unix.AnyCPU.${configuration};\${WORKSPACE}/bin/AnyOS.AnyCPU.${configuration}\" \\
                --coreFxNativeBinDir=\"\${WORKSPACE}/bin/${osGroup}.${architecture}.${configuration}\"""")
            }
        }

        Utilities.setMachineAffinity(newJob, os, 'latest-or-auto') // Just run against Linux VM’s for now.

        // Save machinedata.json to /artifact/bin/ Jenkins dir
        def archiveSettings = new ArchivalSettings()
        archiveSettings.addFiles('sandbox/perf-*.xml')
        archiveSettings.addFiles('machinedata.json')
        Utilities.addArchival(newJob, archiveSettings)

        Utilities.standardJobSetup(newJob, project, isPR, "*/${branch}")

        // For perf, we need to keep the run results longer
        newJob.with {
            // Enable the log rotator
            logRotator {
                artifactDaysToKeep(7)
                daysToKeep(300)
                artifactNumToKeep(25)
                numToKeep(1000)
            }
        }
        if (isPR) {
            TriggerBuilder builder = TriggerBuilder.triggerOnPullRequest()
            builder.setGithubContext("${os} Perf Tests")
            builder.triggerOnlyOnComment()
            builder.setCustomTriggerPhrase("(?i).*test\\W+${os}\\W+perf.*")
            builder.triggerForBranch(branch)
            builder.emitTrigger(newJob)
        }
        else {
            // Set a push trigger
            TriggerBuilder builder = TriggerBuilder.triggerOnCommit()
            builder.emitTrigger(newJob)
        }
    } // os
} // isPR

[true, false].each { isPR ->
    ['Windows_NT'].each { os ->
        def newJob = job(Utilities.getFullJobName(project, "perf_coreclr_microbenchmarks_${os}", isPR)) {
            // Set the label.
            steps {
                    // Batch
					batchFile("C:\\Tools\\nuget.exe install Microsoft.BenchView.JSONFormat -Source http://benchviewtestfeed.azurewebsites.net/nuget -OutputDirectory C:\\tools -Prerelease -version 0.1.0-pre015")
                    batchFile("python C:\\tools\\Microsoft.BenchView.JSONFormat.0.1.0-pre015\\tools\\machinedata.py")
                    batchFile("set __TestIntermediateDir=int&&build.cmd release x64")
                    batchFile("tests\\runtest.cmd release x64")
                    batchFile("tests\\scripts\\run-xunit-perf.cmd -arch x64 -configuration Release -testBinLoc bin\tests\Windows_NT.x64.Release\Jit\Performance\CodeQuality")
            }
        }

        Utilities.setMachineAffinity(newJob, os, 'latest-or-auto-elevated') // Just run against Windows_NT VM’s for now.
        Utilities.standardJobSetup(newJob, project, isPR, "*/${branch}")
        if (isPR) {
            Utilities.addGithubPRTriggerForBranch(newJob, branch, "${os} JIT Code Quality Perf Tests") // Add a PR trigger.
        }
        else {
            // Set a push trigger
            Utilities.addGithubPushTrigger(newJob)
        }
    }
}
