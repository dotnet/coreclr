//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

#ifndef __GCALLOCATOR_H
#define __GCALLOCATOR_H
#include "gcpriv.h"

class alloc_list 
{
    BYTE* head;
    BYTE* tail;
public:
    BYTE*& alloc_list_head () { return head;}
    BYTE*& alloc_list_tail () { return tail;}
    alloc_list()
    {
        head = 0; 
        tail = 0; 
    }
};

class allocator 
{
    size_t num_buckets;
    size_t frst_bucket_size;
    alloc_list first_bucket;
    alloc_list* buckets;
    alloc_list& alloc_list_of (unsigned int bn);

public:
    allocator (unsigned int num_b, size_t fbs, alloc_list* b);
    allocator();
    unsigned int number_of_buckets() {return (unsigned int)num_buckets;}

    size_t first_bucket_size() {return frst_bucket_size;}
    BYTE*& alloc_list_head_of (unsigned int bn);
    BYTE*& alloc_list_tail_of (unsigned int bn);
    void clear();
    BOOL discard_if_no_fit_p();

    // This is when we know there's nothing to repair because this free
    // list has never gone through plan phase. Right now it's only used
    // by the background ephemeral sweep when we copy the local free list
    // to gen0's free list.
    //
    // We copy head and tail manually (vs together like copy_to_alloc_list)
    // since we need to copy tail first because when we get the free items off
    // of each bucket we check head first. We also need to copy the
    // smaller buckets first so when gen0 allocation needs to thread
    // smaller items back that bucket is guaranteed to have been full
    // copied.
    void copy_with_no_repair (allocator* allocator_to_copy);

    void unlink_item (unsigned int bucket_number, BYTE* item, BYTE* previous_item, BOOL use_undo_p);
    void thread_item (BYTE* item, size_t size);
    void thread_item_front (BYTE* itme, size_t size);
    void thread_free_item (BYTE* free_item, BYTE*& head, BYTE*& tail);
    void copy_to_alloc_list (alloc_list* toalist);
    void copy_from_alloc_list (alloc_list* fromalist);
    void commit_alloc_list_changes();
};

#endif // __GCALLOCATOR_H