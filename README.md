TCP Hole-Punching
================

Purpose
-----------

To demonstrate hole punching / NAT traversal using the TCP protocol.

UDP hole punching is more well known and TCP hole punching is less supported by different router models, so this is a proof of concept.

Instructions
------------

1. Run the Introducer executable on a publicy reachable server (e.g. an Amazon EC2 instance).

2. Run the Peer executable on both PCs you want to connect.

3. Use a service like cmyip.com to determine the public WAN IPs of each PC, and enter them in each Peer. Hit <ENTER>.

If the connection succeeds, then TCP hole punching just succeeded.

* It helps to lower the firewalls on each PC.
* This will most likely not work if either PC is in a corporate network.
