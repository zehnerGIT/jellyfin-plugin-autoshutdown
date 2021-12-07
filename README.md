# jellyfin-plugin-autoshutdown
<h1 align="center">Jellyfin AutoShutDown Plugin</h1>

## About
This plugin checks the status of the network and the server. After a configured idle time it shuts down your OS.

## Fair warning
This plugin is only tested on Ubuntu, other Linux distributions should work but some extra configuration steps can be necessary. 
Check the documentation of your distribution.
Windows and Unix/Mac should work but are not tested. Android is not implemented (because it needs JNI and root).
**Don't use it on docker**, the plugin can only shutdown your container, not the host.
IP6 or VPN should work, but are also untested.

## OS configuration
The jellyfin user account needs the permission to execute a shutdown.
### Linux (Ubuntu)
	--- add the following line to /etc/sudoers.d/jellyfin-sudoers ---
	jellyfin ALL=(ALL) NOPASSWD: /usr/sbin/shutdown

### other OSes
Google it and send me the configuration steps.

## Plugin configuration
**Interval (in min):**
Timespan (in min) btw. checks. Only integers > 0 are allowed.
Select a value according to the defined checks (more checks need more time to finish)

**Initial delay (in min):**
Wait this timespan (in min) after booting, before the first check interval is executed.
Only necessary, if your system needs to finish some tasks after boot (e.g. server backups or library updates). Leave empty or set to 0 if not needed.

**Executions:**
Shut down the system after this amount of intervals and exit condition not met.
Every time the exit condition is met, the counter resets to 0.

**Ports:**
List of local ports with an open remote connection.
Only integers > 0 are allowed. Only connections from another host (IPs != Jellyfin-IP) are relevant. Separate multiple ports with a single space. Some possible ports:
	22		SSH
	445		Samba
	8096	Jellyfin 

**Hosts:**
List of remote hosts to ping.
IPs or URLs are allowed. Separate multiple hosts with a single space. Be careful with android devices: most of them can be pinged, even in standby.

## Build Process

1. Clone or download this repository

2. Ensure you have .NET Core SDK setup and installed

3. Build plugin with following command.

```sh
dotnet publish --configuration Release --output bin
```
4. Place the resulting file in the `plugins` folder under the program data directory or inside the portable install directory
