#include "stdlib.h"
#include "testlib.h"

static char heap[16 << 10];
static unsigned int i;

void setup() {
    print("hello world");
    print(uint_to_str(float_bits_to_uint(-3.1415f)));
    i = 0;
}

void loop() {
    print(strcat("value: ", uint_to_str(i), heap));
    i++;

    if (i == 4) {
        loop: goto loop;
    }
}
