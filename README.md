# NetworkKit
UDP network architecture with reliable delivery channel for basic communication.

***

## Example

```C#
// Network
var network = new Network();

// Network settings
network.Settings.RequiresApproval = false;
network.Settings.MaxLinks         = 100u;

// Network event
network.OnFailed     += (link, failure) => {};
network.OnUnlinked   += (link, reason)  => {};
network.OnContent    += (link, content) => {};
network.OnApproval   += (link, content) => {};
network.OnRedirected += (link) => {};
network.OnRedirect   += (link) => {};
network.OnLinked     += (link) => {};
network.OnStopped    += () => {};
network.OnStarted    += () => {};

// Starts on a specific port
network.Start(15000);

// Connects to
network.Link("127.0.0.1:15000"); // Or
network.Link("localhost:15000");
```

## Example in unity

```C#
// In the settings set "UseEvents" to true
network.Settings.UseEvents = true;

// Call the method "Event()" every frame
void Update(){
  network.Event();
}
```

***

## MIT License

### Copyright (c) 2019 Rodrigo Martins

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
