This is originally a fork of https://github.com/NiyaShy/XB1ControllerBatteryIndicator but with some changes i preffered

XBox One Controller Battery Indicator
A tray application that shows a battery indicator for an Xbox-ish controller and gives a notification when the battery level drops to (almost) empty.

It was originally written for the XBox One controller since Microsoft dropped all visual hints for low battery, but it should work with any gamepad that can be addressed via XInput (which should be all controllers that work in XBox-controller-enabled games).

When more than one controller is present, the tray icon will cycle through the status display every 5 seconds.

![Tray icon](https://i.imgur.com/rxWAsu8.gif "Tray icon cycling through multiple controllers")

When a controller reaches low battery level, a notification is played if set via tray icon.

Controllers reported as working/being recognized so far:

* XBOne + dongle
* XBOne Elite + dongle
* XBOne S + dongle 
* XBOne S + Bluetooth
* XB360 

Currently known issues/limitations:

* Initial recognition of a newly connected controller can take a while. It will be displayed as "waiting for battery level data" at first but should switch to battery level after ~10 seconds and a button press. (This is a limitation of the XInput API.)
