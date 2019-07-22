#import "AJKit.h"
#import "OneAudience.h"

extern "C" {
    void initOneAudience(char *appKey) {
    	[AJKit init: [NSString stringWithUTF8String:appKey]];
    }
    
    void setDeveloperEmailAddress(char *email) {
        [AJKit setEmailAddress: [NSString stringWithUTF8String:email]];
    }

  	void optoutOneAudience() {
        [AJKit optOut];
    }
}
