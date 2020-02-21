# RTZP samples

The Basic sample is just an RTZP (a zip archive with extension .rtzp) containing with the mandatory ONE rtz file. 
WithAttachments contains "attached" files that are referred to in extension elements within the RTZ.
In both cases the "source" RTZ is included. 

# How to make RTZP manually

Arrange files appropriately and use a tool such as 7zip to make into a .zip archive and then rename to .zip

[7zip](https://www.7-zip.org/) is free and works well. 

# How they were made with RTZ tool

## Command line for a basic sample

rtz -zip C:\GitHub\RTZ\SampleFiles\Rtzp\Basic\defaultwaypoint.rtz

result:

RTZP will include:
        C:\GitHub\RTZ\SampleFiles\Rtzp\Basic\defaultwaypoint.rtz

Destination C:\GitHub\RTZ\SampleFiles\Rtzp\Basic\defaultwaypoint.rtzp
Successful

## Command line for sample with attachments

rtz -zip C:\GitHub\RTZ\SampleFiles\Rtzp\WithAttachments\source\rtzp_with_attachments.rtz C:\GitHub\RTZ\SampleFiles\Rtzp\WithAttachments\rtzp_with_attachments.rtzp
RTZP will include:
        C:\GitHub\RTZ\SampleFiles\Rtzp\WithAttachments\source\rtzp_with_attachments.rtz
        myAttachment.txt
        mySecondAttachment.txt
        subfolder/myThirdAttachment.txt

Absent attachments?
        myMissingAttachment.txt

Destination C:\GitHub\RTZ\SampleFiles\Rtzp\WithAttachments\rtzp_with_attachments.rtzp
Successful
