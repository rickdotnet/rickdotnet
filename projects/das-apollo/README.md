# DasApollo

An Apollo demo that listens for and sends daskeyboard signals

## Keyboard

This was tested with a [daskeyboard 5QS](https://www.daskeyboard.com/p/5qs-smart-rgb-mechanical-keyboard/)

## Run Demo

Das you have a daskeyboard? The demo consists of a listening side (monitor) and a publishing side (publisher). The demo is currently configured to use a local NATS server for distributed signaling. Comment out the `.AddNatsProvider(...)` code in [Startup.cs](/src/DasMonitor/Startup.cs#L25) if you'd like to use an in-memory provider.
