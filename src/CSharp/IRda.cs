// Copyright (c) 2020 Michael Chen
// Licensed under the MIT License -
// https://github.com/foldda/rda/blob/main/LICENSE

namespace Charian
{
    /*
     * RDA-Serializable object implements this interface to ensure 
     * 1) its properties can be stored into an resulting RDA object,  
     * 2) used a provided RDA to restore this object's properties' values.  
     * 
     */

    public interface IRda
    {
        /// <summary>
        /// Stores properties into the RDA.
        /// </summary>
        /// <returns>An Rda instance that carries the properties of this object.</returns>
        Rda ToRda();

        /// <summary>
        /// Populate properties with the values from the RDA
        /// </summary>
        /// <param name="rda">An Rda instance that carries the properties of an object to be restored.</param>
        /// <returns>An IRda instance that carries the result of the conversion. Eg, it can be the restored the object itself, or an "acknowledgement" Rda object carrying validation errors</returns>
        IRda FromRda(Rda rda);
    }
}

