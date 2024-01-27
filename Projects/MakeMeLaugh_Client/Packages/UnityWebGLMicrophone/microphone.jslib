var LibraryMicrophone =
    {
        $microphoneWorker:
            {
                initResult: "not initialized",
                capture: false,
                buffers: [],
            },

        MicrophoneWebGL_Init: function (bufferSize, numberOfChannels) {
            var mw = microphoneWorker;

            mw.initResult = "pending";

            navigator.mediaDevices.getUserMedia({audio: true})
                .then(stream => {
                    mw.stream = stream; // we need to keep this alive or the gc kills it on Firefox
                    
                    mw.sampleRate = stream.getAudioTracks()[0].getSettings().sampleRate;

                    var audioContext = new window.AudioContext;

                    var scriptNode = audioContext.createScriptProcessor(bufferSize, numberOfChannels, numberOfChannels);
                    scriptNode.onaudioprocess = function (e) {
                        if (!mw.capture) return;
                        for (var channel = 0; channel < e.inputBuffer.numberOfChannels; ++channel) {
                            let buffer = e.inputBuffer.getChannelData(channel);
                            let copy = new Float32Array(buffer.length);
                            copy.set(buffer);
                            mw.buffers.push(copy);
                        }
                    };

                    var input = audioContext.createMediaStreamSource(stream);
                    input.connect(scriptNode);

                    var sink = audioContext.createMediaStreamDestination();
                    scriptNode.connect(sink);

                    mw.initResult = "ready";
                })
                .catch(e => {
                    mw.initResult = "getUserMedia error: " + e;
                });
        },

        MicrophoneWebGL_GetInitResult: function () {
            var bufferSize = lengthBytesUTF8(microphoneWorker.initResult) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(microphoneWorker.initResult, buffer, bufferSize);
            return buffer;
        },

        MicrophoneWebGL_Start: function () {
            microphoneWorker.capture = true;
        },

        MicrophoneWebGL_Stop: function () {
            microphoneWorker.capture = false;
        },

        MicrophoneWebGL_GetNumBuffers: function () {
            return microphoneWorker.buffers.length;
        },

        MicrophoneWebGL_GetBuffer: function (bufferPtr) {
            if (microphoneWorker.buffers.length == 0) {
                return false;
            }
            HEAPF32.set(microphoneWorker.buffers.shift(), bufferPtr >> 2);
            return true;
        },
        
        MicrophoneWebGL_GetSampleRate: function () {
            return microphoneWorker.sampleRate;
        }
    };

autoAddDeps(LibraryMicrophone, '$microphoneWorker')
mergeInto(LibraryManager.library, LibraryMicrophone);

