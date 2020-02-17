# Test route file conforming to the minimum mandatory elements of schema version 1.0

[Rtz10AllOptional.rtz](./Rtz10AllOptional.rtz)

[Picture](./AllOptionElements.png)

# Notes

This is for task T7 for sub-group working on ECDIS WG development of the RTZ1.1 standard

T7: Create a test route file for Clause 6.9.8 (f)

Text from developing standard reads:

6.9.8 Route Exchange
f)	Import a route conforming to schema version 1.1 containing all optional elements. Confirm by observation that the file imports without error and that all parts of the schema identified in the user manual as supported have correct values.

# TO DISCUSS: 

Many of the examples in the Working document have capital letters starting element names e.g \<Waypoints> rather than \<waypoints> in compliance with schema. The document is all over the place here.

Some examples is the document contain typo "portsizeXTD" rather than "portsideXTD"

Document contains use of "," for decimal point symbol - I assume XML uses "." - inconsistency. What is CIRM approach to that?

Schema 1.0 at least does not check for non-standard extensions

stm project seemed to have put extensions in route info which is not allowed but is in keeping with the norm so probably should be added to the list of things that can be extended

in line with general vagueness around element/attribute case there is doubt over waypointId vs. waypointID for schedule elements

# Checks to do

- schedules stuff
- check that schedules move forward (would need to have RL and GC calcs in s/w to actually check schedules)
