# Testing with Azure DevOps

We enabled Azure DevOps for pull request validation. You can now see the new "badge" **coreclr-ci** that was added on a pull request page.

We also disabled triggering Windows_NT Ubuntu arm32/arm64 innerloop jobs from Jenkins since they are running on limited hardware.
The results for those jobs can be found in the new CI system. 
Other legs are running as usual and we will continue monitoring them to validate consistency between Jenkins and AzDev test jobs.

# How to find the test results in Azure DevOps?

If you click on coreclr-ci Details link 
![coreclr-ci-badge](coreclr-ci-badge.png)

this would take you to (Azure DevOps Portal)[https://dev.azure.com/dnceng/public/_build?definitionId=228] for dotnet/coreclr repo where you can see on the left Build and Test jobs that were triggered.

![azdev-portal](azdev-portal.png)

You can select a job and see what step it is currently running.

![azdev-job-step](azdev-job-step.png)

All the test jobs have "Send tests to Helix" step which uploads built product binaries and test artifacts to Helix machines for running concurrently and waits for all tests to finish.
If all tests are passed the step icon turns green. 


If there are any failed tests the icon goes red. To see what tests have failed you need to click on the step link


and then click on 

> Results will be available from https://mc.dot.net/#/user/coreclr-pulls/pr~2Fdotnet~2Fcoreclr~2Frefs~2Fpull~2F22051~2Fmerge/test~2Ffunctional~2Fcli~2Fpri0/20190117.72

which would take you to (Mission Control)[https://mc.dot.net/#/user/coreclr-pulls/pr~2Fdotnet~2Fcoreclr~2Frefs~2Fpull~2F22051~2Fmerge/test~2Ffunctional~2Fcli~2Fpri0/20190117.72]

You will see the matrix of test results where columns correspond to combinations of different operating systems and architectures (e.g., Fedora.28.Amd64.Open-x64, Windows.10.Amd64.Open-x86) while rows correspond to different collections of tests (e.g.,  JIT.Performance, GC.Regressions).

If there are failing tests the will appear at the bottom 



