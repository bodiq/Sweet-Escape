#import "GoogleSignInFunctionDefs.h"
#import "GoogleSignIn/GoogleSignIn.h"
#import "UserNotifications/UserNotifications.h"

#pragma clang diagnostic push
#pragma ide diagnostic ignored "OCUnusedGlobalDeclarationInspection"

extern "C" {
	char* cStringCopy(const char* string) {
		char *res = (char *) malloc(strlen(string) + 1);
		strcpy(res, string);
		return res;
	}
	
	char* createCStringFrom(NSString* string) {
		if (!string) {
			string = @"";
		}
		
		return cStringCopy([string UTF8String]);
	}

	NSString* createNSStringFrom(const char* cstring) {
		return [NSString stringWithUTF8String:(cstring ?: "")];
	}

	void _promptGoogleSignIn(const char* clientId, ActionStringCallbackDelegate onSuccess, void *onSuccessActionPtr, ActionStringCallbackDelegate onError, void *onErrorActionPtr) {
        [GIDSignIn sharedInstance].configuration = [[GIDConfiguration alloc] initWithClientID:createNSStringFrom(clientId)];
		
		[[GIDSignIn sharedInstance] restorePreviousSignInWithCompletion:^(GIDGoogleUser * _Nullable user, NSError * _Nullable error) {
			if (user != nil) {
				onSuccess(onSuccessActionPtr, createCStringFrom(user.idToken.tokenString));
			} else {
				[[GIDSignIn sharedInstance] signInWithPresentingViewController:UnityGetGLViewController() completion:^(GIDSignInResult * _Nullable result, NSError * _Nullable error) {
					if (result.user != nil) {
						onSuccess(onSuccessActionPtr, createCStringFrom(result.user.idToken.tokenString));
					} else {
						onError(onErrorActionPtr, createCStringFrom(error.localizedDescription));
					}
				}];
			}
		}];
	}

	void _logout() {
		[[GIDSignIn sharedInstance] signOut];
	}

    void _checkNotificationPermission(ActionBoolCallbackDelegate onCompleted, void *onCompletedActionPtr) {
        UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
        UNAuthorizationOptions options = (UNAuthorizationOptionAlert + UNAuthorizationOptionSound);
        [center requestAuthorizationWithOptions:options completionHandler:^(BOOL granted, NSError * _Nullable error) {
            if (granted)
                [[UIApplication sharedApplication] registerForRemoteNotifications];
            
            onCompleted(onCompletedActionPtr, granted);
        }];
    }
}
