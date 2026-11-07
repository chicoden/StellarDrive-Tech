#ifndef _TESTLIB_H
#define _TESTLIB_H

const char* strcat(const char* lhs, const char* rhs, char* result) {
    char* pos = result;
    while (*lhs) *pos++ = *lhs++;
    while (*rhs) *pos++ = *rhs++;
    *pos = '\0';
    return result;
}

unsigned int float_bits_to_uint(float value) {
    return (union {
        float value;
        unsigned int bits;
    }){
        .value = value
    }.bits;
}

const char* uint_to_str(unsigned int value) {
    static char buffer[16];
    char* pos = buffer;
    do {
        *pos++ = value % 10 + '0';
        value /= 10;
    } while (value != 0);

    for (char *l = buffer, *r = pos - 1; l < r; l++, r--) {
        char tmp = *l;
        *l = *r;
        *r = tmp;
    }

    *pos = '\0';
    return buffer;
}

#endif
