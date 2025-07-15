// PDFExportPlugin.mm
// Place this file in Assets/Plugins/iOS/ folder in your Unity project

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

extern "C" {
    void _SavePdfToDocuments(unsigned char* pdfData, int dataLength, const char* fileName);
    void _OpenPdfWithActivityViewController(const char* filePath);
}

void _SavePdfToDocuments(unsigned char* pdfData, int dataLength, const char* fileName) {
    @autoreleasepool {
        NSLog(@"[iOS Plugin] Starting PDF save process");
        
        // Validate input parameters
        if (pdfData == NULL) {
            NSLog(@"[iOS Plugin] Error: PDF data is NULL");
            return;
        }
        
        if (dataLength <= 0) {
            NSLog(@"[iOS Plugin] Error: Data length is %d", dataLength);
            return;
        }
        
        if (fileName == NULL) {
            NSLog(@"[iOS Plugin] Error: File name is NULL");
            return;
        }
        
        NSString* fileNameString = [NSString stringWithUTF8String:fileName];
        NSLog(@"[iOS Plugin] File name: %@", fileNameString);
        
        NSData* data = [NSData dataWithBytes:pdfData length:dataLength];
        NSLog(@"[iOS Plugin] Created NSData with length: %lu", (unsigned long)[data length]);
        
        // Get Documents directory path
        NSArray* paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
        NSString* documentsDirectory = [paths objectAtIndex:0];
        NSLog(@"[iOS Plugin] Documents directory: %@", documentsDirectory);
        
        // Create XHeroScan/PDF subdirectory if it doesn't exist
        NSString* subDirectory = [documentsDirectory stringByAppendingPathComponent:@"XHeroScan/PDF"];
        NSFileManager* fileManager = [NSFileManager defaultManager];
        
        if (![fileManager fileExistsAtPath:subDirectory]) {
            NSError* error = nil;
            BOOL created = [fileManager createDirectoryAtPath:subDirectory withIntermediateDirectories:YES attributes:nil error:&error];
            if (error) {
                NSLog(@"[iOS Plugin] Error creating directory: %@", error.localizedDescription);
                return;
            }
            NSLog(@"[iOS Plugin] Created directory: %@", subDirectory);
        }
        
        // Create full file path
        NSString* filePath = [subDirectory stringByAppendingPathComponent:fileNameString];
        NSLog(@"[iOS Plugin] Full file path: %@", filePath);
        
        // Write PDF data to file
        BOOL success = [data writeToFile:filePath atomically:YES];
        
        if (success) {
            NSLog(@"[iOS Plugin] PDF saved successfully to: %@", filePath);
            
            // Automatically open the PDF with activity view controller
            _OpenPdfWithActivityViewController([filePath UTF8String]);
        } else {
            NSLog(@"[iOS Plugin] Failed to save PDF to: %@", filePath);
        }
    }
}

void _OpenPdfWithActivityViewController(const char* filePath) {
    @autoreleasepool {
        NSLog(@"[iOS Plugin] Opening PDF with activity view controller");
        
        NSString* filePathString = [NSString stringWithUTF8String:filePath];
        NSURL* fileURL = [NSURL fileURLWithPath:filePathString];
        
        // Check if file exists
        if (![[NSFileManager defaultManager] fileExistsAtPath:filePathString]) {
            NSLog(@"[iOS Plugin] Error: File does not exist at path: %@", filePathString);
            return;
        }
        
        dispatch_async(dispatch_get_main_queue(), ^{
            UIViewController* rootViewController = [UIApplication sharedApplication].keyWindow.rootViewController;
            
            // Find the top-most view controller
            while (rootViewController.presentedViewController) {
                rootViewController = rootViewController.presentedViewController;
            }
            
            NSLog(@"[iOS Plugin] Root view controller: %@", rootViewController);
            
            // Create activity view controller
            UIActivityViewController* activityVC = [[UIActivityViewController alloc] 
                                                   initWithActivityItems:@[fileURL] 
                                                   applicationActivities:nil];
            
            // Configure for iPad
            if ([UIDevice currentDevice].userInterfaceIdiom == UIUserInterfaceIdiomPad) {
                activityVC.popoverPresentationController.sourceView = rootViewController.view;
                activityVC.popoverPresentationController.sourceRect = CGRectMake(
                    rootViewController.view.bounds.size.width / 2.0,
                    rootViewController.view.bounds.size.height / 2.0,
                    1.0, 1.0
                );
            }
            
            // Present the activity view controller
            [rootViewController presentViewController:activityVC animated:YES completion:^{
                NSLog(@"[iOS Plugin] Activity view controller presented successfully");
            }];
        });
    }
}