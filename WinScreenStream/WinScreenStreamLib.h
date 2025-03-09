#pragma once

#ifdef WINSCREENSTREAMLIB_EXPORTS
#define WINSCREENSTREAMLIB_API __declspec(dllexport)
#else
#define WINSCREENSTREAMLIB_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

    // Simple descriptor for a display/monitor
    typedef struct DisplayInfo {
        int id;      // identifier used by our library
        char name[128];
        int width;
        int height;
    } DisplayInfo;

    // Callback signature for receiving frames (ARGB or RGBA)
    typedef void (*CaptureFrameCallback)(
        const unsigned char* pixels,
        int width,
        int height,
        void* userContext
        );

    // Returns the number of active displays. Fills 'infos' up to maxCount.
    WINSCREENSTREAMLIB_API int GetActiveDisplays(DisplayInfo* infos, int maxCount);

    // Start capture on the display 'displayId'; returns 0 on success.
    WINSCREENSTREAMLIB_API int StartCapture(
        int displayId,
        CaptureFrameCallback callback,
        void* userContext
    );

    // Stop capture. If capturing is active, it stops the thread and releases resources.
    WINSCREENSTREAMLIB_API void StopCapture();

#ifdef __cplusplus
}
#endif