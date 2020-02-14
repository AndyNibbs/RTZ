# RTZ
Utilities and knowledge related to RTZ route exchange format (part of IEC61174:2015, ECDIS)

## RTZP data containers

The RTZ tool can be used to create RTZP wrappers and unwrap an RTZP file. Usage is:

RTZ -zip rtz-filename [rtzp filename]\
*Makes an RTZP container with the RTZ file and attachments*

RTZ -unzip rtzp-filename [folder]\
*Extracts contents of an RTZP to a given folder or defaults to filename of RTZP as folder name*

Assumptions:
- When zipping it is tolerant of absent attachments
- When zipping it assumes attachments are rooted in same folder as RTZ file (if the URI is rtz://attachment.txt then attachment.txt is in the same folder as the RTZ file that references it)
- When zipping it will cope with attachments in subfolders (rtz://subfolder/att.txt)
- Attachment URIs can be in either an element or an attribute, e.g.\
 	\<element>rtz://doc.pdf\</element>\
	\<property name="attachment" value="rtz://notes.docx" />
- I think the developing 1.1 standard *suggests* attachments would only be in manufacturer extensions but this is not reflected in code. 
- An attachment URI would be the only contents of an attribute or element, i.e. **not** \
    \<element>**Here is my attachment rtz://text.txt I urge you to read it**\</element> 

## Checking RTZ

RTZ.exe can check a file against the standard (or at least an interpretation of the standard). This includes checking against the XML schema XSD (which is built into the software). Some additional checks will go beyond what is easily possible with XSD and some of simple (e.g. check file size).

RTZ -check <rtz or rtzp filename|folder containing multiple files> [destination file for report]

e.g. RTZ -check c:\myRtzFiles c:\myCheckReport.txt

### Non-exhaustive, best efforts list of checks...
- Standard schema check
- Check size is below 400kb limit
- Check that waypoint ids are unique
- Checking existence and values of version attribute prior to schema check
- Warn of very short route legs

### There's a fairly lengthy TODO list:

- Support checking RTZ when wrapped in RTZP.
- Build in RTZ 1.1 XSD.
- Warnings around "sane" RouteInfo values beyond what XSD checks
- etc etc..







## General

RTZ is a .net core 3.1 console app so it should run on Linux/MacOS as well as Windows. 


Andy Nibbs
