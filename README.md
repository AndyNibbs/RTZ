# RTZ
Utilities and knowledge related to RTZ route exchange format (part of IEC61174:2015, ECDIS)

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

RTZ is a .net core 3.1 console app so it should run on Linux/MacOS as well as Windows. 


Andy Nibbs
