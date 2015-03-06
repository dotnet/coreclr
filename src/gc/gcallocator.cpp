//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

#include "gc.h"
#include "gcallocator.h"

allocator::allocator (unsigned int num_b, size_t fbs, alloc_list* b)
{
    assert (num_b < MAX_BUCKET_COUNT);
    num_buckets = num_b;
    frst_bucket_size = fbs;
    buckets = b;
}
	
allocator::allocator() 
{
        num_buckets = 1;
        frst_bucket_size = SIZE_T_MAX;
}

alloc_list& allocator::alloc_list_of (unsigned int bn)
{
    assert (bn < num_buckets);
    if (bn == 0)
        return first_bucket;
    else
        return buckets [bn-1];
}

void allocator::unlink_item (unsigned int bn, BYTE* item, BYTE* prev_item, BOOL use_undo_p)
{
    //unlink the free_item
    alloc_list* al = &alloc_list_of (bn);
    if (prev_item)
    {
        if (use_undo_p && (free_list_undo (prev_item) == UNDO_EMPTY))
        {
            free_list_undo (prev_item) = item;
        }
        free_list_slot (prev_item) = free_list_slot(item);
    }
    else
    {
        al->alloc_list_head() = (BYTE*)free_list_slot(item);
    }
    if (al->alloc_list_tail() == item)
    {
        al->alloc_list_tail() = prev_item;
    }
}

void allocator::clear()
{
    for (unsigned int i = 0; i < num_buckets; i++)
    {
        alloc_list_head_of (i) = 0;
        alloc_list_tail_of (i) = 0;
    }
}

//always thread to the end.
void allocator::thread_free_item (BYTE* item, BYTE*& head, BYTE*& tail)
{
    free_list_slot (item) = 0;
    free_list_undo (item) = UNDO_EMPTY;
    assert (item != head);

    if (head == 0)
    {
       head = item;
    }
    //TODO: This shouldn't happen anymore - verify that's the case.
    //the following is necessary because the last free element
    //may have been truncated, and tail isn't updated.
    else if (free_list_slot (head) == 0)
    {
        free_list_slot (head) = item;
    }
    else
    {
        assert (item != tail);
        assert (free_list_slot(tail) == 0);
        free_list_slot (tail) = item;
    }
    tail = item;
}

void allocator::thread_item (BYTE* item, size_t size)
{
    size_t sz = frst_bucket_size;
    unsigned int a_l_number = 0; 

    for (; a_l_number < (num_buckets-1); a_l_number++)
    {
        if (size < sz)
        {
            break;
        }
        sz = sz * 2;
    }
    alloc_list* al = &alloc_list_of (a_l_number);
    thread_free_item (item, 
                      al->alloc_list_head(),
                      al->alloc_list_tail());
}

void allocator::thread_item_front (BYTE* item, size_t size)
{
    //find right free list
    size_t sz = frst_bucket_size;
    unsigned int a_l_number = 0; 
    for (; a_l_number < (num_buckets-1); a_l_number++)
    {
        if (size < sz)
        {
            break;
        }
        sz = sz * 2;
    }
    alloc_list* al = &alloc_list_of (a_l_number);
    free_list_slot (item) = al->alloc_list_head();
    free_list_undo (item) = UNDO_EMPTY;
    if (al->alloc_list_tail() == 0)
    {
        al->alloc_list_tail() = al->alloc_list_head();
    }
    al->alloc_list_head() = item;
    if (al->alloc_list_tail() == 0)
    {
        al->alloc_list_tail() = item;
    }
}

void allocator::copy_to_alloc_list (alloc_list* toalist)
{
    for (unsigned int i = 0; i < num_buckets; i++)
    {
        toalist [i] = alloc_list_of (i);
    }
}

void allocator::copy_from_alloc_list (alloc_list* fromalist)
{
    BOOL repair_list = !discard_if_no_fit_p ();
    for (unsigned int i = 0; i < num_buckets; i++)
    {
        alloc_list_of (i) = fromalist [i];
        if (repair_list)
        {
            //repair the the list
            //new items may have been added during the plan phase 
            //items may have been unlinked. 
            BYTE* free_item = alloc_list_head_of (i);
            while (free_item)
            {
                assert (((CObjectHeader*)free_item)->IsFree());
                if ((free_list_undo (free_item) != UNDO_EMPTY))
                {
                    free_list_slot (free_item) = free_list_undo (free_item);
                    free_list_undo (free_item) = UNDO_EMPTY;
                }

                free_item = free_list_slot (free_item);
            }
        }
#ifdef DEBUG
        BYTE* tail_item = alloc_list_tail_of (i);
        assert ((tail_item == 0) || (free_list_slot (tail_item) == 0));
#endif
    }
}

void allocator::commit_alloc_list_changes()
{
    BOOL repair_list = !discard_if_no_fit_p ();
    if (repair_list)
    {
        for (unsigned int i = 0; i < num_buckets; i++)
        {
            //remove the undo info from list. 
            BYTE* free_item = alloc_list_head_of (i);
            while (free_item)
            {
                assert (((CObjectHeader*)free_item)->IsFree());
                free_list_undo (free_item) = UNDO_EMPTY;
                free_item = free_list_slot (free_item);
            }
        }
    }
}
void allocator::copy_with_no_repair (allocator* allocator_to_copy)
{
    assert (num_buckets == allocator_to_copy->number_of_buckets());
    for (unsigned int i = 0; i < num_buckets; i++)
    {
        alloc_list* al = &(allocator_to_copy->alloc_list_of (i));
        alloc_list_tail_of(i) = al->alloc_list_tail();
        alloc_list_head_of(i) = al->alloc_list_head();
    }
}

BYTE*& allocator::alloc_list_head_of (unsigned int bn)
{
    return alloc_list_of (bn).alloc_list_head();
}
BYTE*& allocator::alloc_list_tail_of (unsigned int bn)
{
    return alloc_list_of (bn).alloc_list_tail();
}
BOOL allocator::discard_if_no_fit_p()
{
    return (num_buckets == 1);
}