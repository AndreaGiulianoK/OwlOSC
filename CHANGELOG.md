# OwlOSC CHANGELOG

All notable changes to this project will be documented in this file.

## [1.7] 2022.09.30
- Added Unity Project and Package
- Changes to the handling of the listener's message queue

## [1.6] 2022.09.28
- Added `ToString` override to all OSC custom type
- Message `ToString` now return also data type
- Added content and build sections to readme.md

## [1.5] 2022.08.19
- Address wildcard
- Immediate Addres callback

## [1.4] 2022.08.18

- Received message address validation
- Bundle constructor

## [1.3] 2022.08.17

- IDisposable for sender/listener

## [1.2] 2022.08.17

- Async address callback
- Validate address util
- Test program options change
- Tested in Unity

## [1.1] 2022.08.16

- Added test console program in `OwlOSC.Test` (send / receive, receive loop Async, speedtest, send / receive file)
- Message / Bundle proper 'ToString' override
- UDP send size limit (64k)
- Simple Bundle recognition with boolean in OSCPacket

## [1.0] 2022.08.14

- Base OSC from SharpOSC and rename namespace