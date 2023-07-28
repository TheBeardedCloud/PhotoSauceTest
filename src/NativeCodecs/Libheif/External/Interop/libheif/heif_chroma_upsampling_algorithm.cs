// Copyright © Clinton Ingram and Contributors. Licensed under the MIT License (MIT).

// Ported from libheif headers (heif.h)
// Original source Copyright (c) struktur AG, Dirk Farin <farin@struktur.de>
// See third-party-notices in the repository root for more information.

namespace PhotoSauce.Interop.Libheif;

internal enum heif_chroma_upsampling_algorithm
{
    heif_chroma_upsampling_nearest_neighbor = 1,
    heif_chroma_upsampling_bilinear = 2,
}
