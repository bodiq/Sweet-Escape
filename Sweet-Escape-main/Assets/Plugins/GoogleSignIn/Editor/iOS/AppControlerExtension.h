#import "UnityAppController.h"

@interface AppControllerExtension : UnityAppController
{
}
@end

- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options;

@implementation AppControllerExtension

- (BOOL)application:(UIApplication*)app
			openURL:(NSURL*)url
			options:(NSDictionary<NSString*, id>*)options {
	return [GIDSignIn.sharedInstance handleURL:url];
}

@end


IMPL_APP_CONTROLLER_SUBCLASS(AppControllerExtension)
