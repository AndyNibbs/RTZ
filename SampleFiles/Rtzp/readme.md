

# Command line for a basic sample

rtz -zip C:\GitHub\RTZ\SampleFiles\Rtzp\Basic\defaultwaypoint.rtz

result:

RTZP will include:
        C:\GitHub\RTZ\SampleFiles\Rtzp\Basic\defaultwaypoint.rtz

Destination C:\GitHub\RTZ\SampleFiles\Rtzp\Basic\defaultwaypoint.rtzp
Successful

# Command line for sample with attachments

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