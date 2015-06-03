#
# ScriptParamAllTypes.ps1
#

Param(
    [parameter(Mandatory = $false)]
    [string]
    $StringType,

    [parameter(Mandatory = $false)]
    [char]
    $CharType,

    [Parameter(Mandatory=$false)] 
    [byte] 
    $ByteType,

    [Parameter(Mandatory=$false)] 
    [int] 
    [ValidateRange(21,65)] 
    $IntType,
     
    [Parameter(ParameterSetName="Set1")]
    [long]
    $LongType,
     
    [Parameter(ParameterSetName="Set1")]
    [bool]
    $BoolType,
    
    [Parameter(ParameterSetName="Set2")]  
    [Switch]
    $SwitchType,

    [Parameter(ParameterSetName="Set2")]
    [decimal]
    $DecimalType,

    [Parameter()]
    [single]
    $SingleType,

    [Parameter()]
    [double]
    $DoubleType,


    [Parameter()]
    [DateTime]
    $DateTimeType,

    [Parameter()]
    [xml]
    $XmlType,

    [Parameter()]
    [int[]]
    $ArrayIntType,

    [Parameter()]
    [string[]]
    $ArrayStringType,

    [Parameter()]
    [hashtable]
    $HashtableType,

    [Parameter()]
    [SecureString]
    $Password,

    [Parameter()]
    [PSCredential]
    $PSCredentialType
) 
   
                      
function Write-Status([string]$msg) { 
    $timeStamp = Get-Date -Format o
    $msg = "[" + $timeStamp + "]: " + $msg
    Write-Host $msg -ForegroundColor Green
}

Write-Status ('$StringType = ' + $StringType)
Write-Status ('$CharType = ' + $CharType)
Write-Status ('$ByteType = ' + $ByteType)
Write-Status ('$IntType =  ' + $IntType)
Write-Status ('$LongType = ' + $LongType)
Write-Status ('$BoolType = ' + $BoolType)
Write-Status ('$SwitchType = ' + $SwitchType)
Write-Status ('$SingleType = ' + $SingleType)
Write-Status ('$DoubleType = ' + $DoubleType)
Write-Status ('$DecimalType = ' + $DecimalType)
Write-Status ('$DateTimeType = ' + $DateTimeType)
Write-Status ('$ArrayIntType = ' + $ArrayIntType)
Write-Status ('$ArrayStringType = ' + $ArrayStringType)
Write-Status $XmlType
Write-Status $HashtableType