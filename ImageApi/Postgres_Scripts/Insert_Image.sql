INSERT INTO public.image_table (img_hash_full, img_hash_partial, img_data) 
VALUES (@img_hash_full, @img_hash_partial, @img_data) 
ON CONFLICT (img_hash_full) DO NOTHING