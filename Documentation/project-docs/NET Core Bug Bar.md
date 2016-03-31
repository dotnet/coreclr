##.NET Core Bug Bars##
Below are the primary criteria used to determine if a fix is appropriate as the .NET Core project works toward release. Escrow is mostly self-explanatory ... only fixes for "ship stopping" issues are accepted. `Ask Mode` needs a little explanation. In this mode a group examines every candidate fix to ensure they satisfy the priorities of the release and do not represent undue risk to stability. 

###RC2 Ask Mode###
- Issues which block RC2 sign-off (tenet, security, reliability, stability, etc)
- Partner blocking bug
- Key scenario blocking bug
- Late feature work which has been explicitly approved by submitting team's management and NetCoreShip.
- CLI stabilization support

###RC2 Escrow###
- Key RC2 scenario and ship blocking issues.
	
###RTW Ask Mode###
- Issues which block RTW sign-off (tenet, security, reliability, stability, etc)
- Critical partner blocking bug
- Key scenario blocking bug
- Late feature work which has been explicitly approved by submitting team's management, NetCoreShip and .NET Directors.

###RTW Escrow###
- Ship blocking issues