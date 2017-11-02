import jobs.generation.JobReport;
import jobs.generation.Utilities;
import org.dotnet.ci.pipelines.Pipeline

// The input project name (e.g. dotnet/corefx)
def project = GithubProject
// The input branch name (e.g. master)
def branch = GithubBranchName

// **************************
// Define innerloop testing. Any configuration in ForPR will run for every PR but all other configurations
// will have a trigger that can be
// **************************

def perfPipeline = Pipeline.createPipelineForGithub(this, project, branch, 'buildpipeline/perf-pipeline.groovy')

def triggerName = "Perf Build and Test"
def pipeline = perfPipeline

// If we were using parameters for the pipeline job, we would define an array of parameter pairs
// and pass that array as a parameter to the trigger functions. Ie:
// def params = ['CGroup':'Release',
//               'AGroup':'x64',
//               'OGroup':'Windows_NT']
// pipeline.triggerPipelinOnGithubPRComment(triggerName, params)

pipeline.triggerPipelineOnEveryGithubPR(triggerName)
pipeline.triggerPipelineOnGithubPush()

['perf', 'throughput', 'jit_bench', 'illink'].each { scenario ->
    ['x64', 'x86'].each { arch ->
        ['min_opt', 'full_opt', 'tiered'].each { opt_level ->
            ['pgo', 'nopgo'].each { pgo_enabled ->
                ['windows_nt', 'linux'].each { os_group ->
                    if (!(os_group == 'linux' && ((arch == 'x86') || (opt_level == 'tiered') || (scenario == 'jit_bench') || (scenario == 'illink')))) {
                        def params = ['Scenario': scenario,
                                      'AGroup': arch,
                                      'OGroup': os_group,
                                      'OptGroup': opt_level,
                                      'PgoGroup': pgo_enabled]
                        triggerName = "${scenario} ${os_group} ${arch} ${opt_level} ${pgo_enabled}"
                        pipeline.triggerPipelineOnGitHubPRComment(triggerName, params)
                    }
                }
            }
        }
    }
}
