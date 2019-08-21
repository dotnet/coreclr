# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.
#
# Run makeSmOpcodeDef.pl and copy/paste the output to smopcode.def

use strict;

my $filename = "smopcodemap.def";

if (!open(INPUTFILE, $filename)) {
   die "Could not open input file $filename.\n";
}

printf("// ==++==\n");
printf("//\n"); 
printf("// Licensed to the .NET Foundation under one or more agreements.\n");
printf("// The .NET Foundation licenses this file to you under the MIT license.\n");
printf("// See the LICENSE file in the project root for more information.\n");
printf("//\n"); 
printf("// ==--==\n");
printf("/*******************************************************************************************\n");
printf(" **                                                                                       **\n");
printf(" ** Auto-generated file. Do NOT modify!                                                   **\n");
printf(" **                                                                                       **\n");
printf(" ** smopcode.def - Opcodes used in the state machine in JIT.                              **\n");
printf(" **                                                                                       **\n");
printf(" ** To generate this file, run \"makeSmOpcodeDef.pl > smopcode.def\"                        **\n");
printf(" **                                                                                       **\n");
printf(" *******************************************************************************************/\n");
printf("\n");                                                                            
printf("//\n");                                                                            
printf("//  SM opcode name                  SM opcode string\n");  
printf("// -------------------------------------------------------------------------------------------\n");

my $count = 0;

my $hash = {};

while (<INPUTFILE>) 
{  
    my $line = $_;

    if ($line =~ /(CEE_\S*),\s*("\S*"),\s*SM_(\S*)\).*/i)
    {
       my $opcode = $1;
       my $smname = $3;

       if ($hash->{$smname}->{'exist'} eq "")
       {
         $hash->{$smname}->{'exist'} = "1";

         my $string = "\"" . lc($smname) . "\"";
         $string =~ s/_/\./g;
         
         $line = "SMOPDEF(SM_" . $smname . ",                 " . $string . ")   // $count";

         print $line . "\n";         
         $count++;
       }
       
    }    
} 

close INPUTFILE;

